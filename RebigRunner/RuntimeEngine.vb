#Region "HEADER COMMENT"
' Main routine that does scripted control of systems.
' Command file consists of one or more of the following commands, one per line:

' MODE <modeType>						- Change defender mode to one of:
'											FILTER
'											IDLE
'											BUMP
' ANALOGVALUE <AnalogNo> <Value>		- Set Analog Value for <AnalogNo> (1-4) to specified value (0-20)
' CLEARALARM							- Clears all active alarms
' VAR $<VARNAME> [ = value ]			- Declare variable VARNAME [optional set variable]
' SIMULATEIO							- Simulate the Analog and Digital values instead of using live data
' GOTO <LABEL>							- Go to the label inside of the file
' PRINT "MESSAGE"						- Prints a message to the log between "s [Can use $<VARNAME> in MESSAGE as well]
' SETWORD <ModbusRegister> <value>		- Sets the word [Short] at <ModbusRegister> to <value>
' SETDWORD <ModbusRegister> <value>		- Sets the double word [Integer] at <ModbusRegister> to <value>
' SETFLOAT <ModbusRegister> <value>		- Sets the Single [Float] at <ModbusRegister> to <value>
' CHECKWORD <ModbusRegister> <value>	- Check if the word at <ModbusRegister> matches <value>
' CHECKDWORD <ModbusRegister> <value>	- Check if the double word at <ModbusRegister> matches <value>
' LOOPTILL <modeType> HH:MM:SS			- Loop for HH:MM:SS looking for <modeType>
' LOOPIN <modeType> HH:MM:SS			- Loop in <modeType> expected for HH:MM:SS
' SCHEDULE <type> <minute>				- Set up one of the following schedules:
'											DRAIN: Schedules a drain in <minute>
'											PRECOAT: Schedules a precoat in <minute>
'											BUMP: Schedules a bump in <minute>
'											BLOCK: Schedules a bump block that starts 1 minute in the past till 29 minutes into the future
' :<LABEL>								- label used for GOTO. Must start with colon
' EXIT									- Exit the command list
' $<VARNAME> += <value>					- Bump <VARNAME> up by <value>
' $<VARNAME> -= <value>					- Bump <VARNAME> down by <value>
' '										- Comment lines
' #										- First line that indicates the Title of the script


' List of obsolete commands that have been commented out of the code

' CHECKMODE <modeType>					- Check if it the current mode is <modeType>
'	- Replaced with LOOPTILL and LOOPIN
' DIGITALVALUE <DiginBit> <Value>		- Set Digital Bit Value for <DiginBit> (1-3) to specified <value> (0 or 1)
'	- Has not been used at all
' SETPARAM <paramNo> <value>			- Set the <paramNo> (1-60) with the desired <value> (CARE MUST BE TAKEN. These values are written to memory)
'	- Has not been used at all
' DELAY HH:MM:SS						- Delay the program for HH:MM:SS
'	- Replaced with LOOPTILL and LOOPIN
' CHECKMODEL <ModbusRegister> <value>	- Checks the <ModbusRegister> to see if it matches the <value>
'	- Has not been used at all
#End Region
Imports System.IO
Imports System.Threading
Imports Newtonsoft.Json

Class RuntimeEngine
#Const Debug		  = 0
#Const DebugScript	  = 0
#Const DebugSkip	  = 0
#Const DebugLinePause = 0

#If DebugSkip = 1 Then
	Dim Line As Integer = 114
#End If

#If DebugLinePause = 1 Then
	Dim pauseLine As Integer = 304
#End If

	Const NK As Integer = 0	' Not Known Addesses/locations

#Region "MODBUS ADDRESS"
	'   NAME											  R	   D7	 SIZE	BYTES					DATA TYPE
	Dim MODBUS_InputMemory()			As Integer = {40000, 3000}	'16     2						M_WORD_PACKED_BINARY
	Dim MODBUS_RelayMemory()			As Integer = {40050, 3003}	'16		2						M_WORD_PACKED_BINARY
	Dim MODBUS_AnalogRaw()				As Integer = {40100, 7778}	' 4     4						M_FLOAT
	Dim MODBUS_AnalogScaled()			As Integer = {40200, 7794}	' 4		4						M_FLOAT
	Dim MODBUS_AnalogCurrentDeltaP()	As Integer = {40300, 7802}	' 1		4						M_FLOAT
	Dim MODBUS_AnalogPreReviveDeltaP()	As Integer = {40402, 7804}	' 1		4						M_FLOAT
	Dim MODBUS_AnalogPostReviveDeltaP()	As Integer = {40404, 7806}	' 1		4						M_FLOAT
	Dim MODBUS_AnalogPreDrainDeltaP()	As Integer = {40406,   NK}	' 1		4						M_FLOAT
	Dim MODBUS_AnalogPostDrainDeltaP()	As Integer = {40408,   NK}	' 1		4						M_FLOAT
	Dim MODBUS_SimulatedDigitalIn()		As Integer = {40500, 3842}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_SimulatedAnalogIn()		As Integer = {40502, 7826}	' 4		4						M_FLOAT
	Dim MODBUS_ScalingAnalogmAmp()		As Integer = {40600, 7870}	' 8		4						M_FLOAT
	Dim MODBUS_AnalogOutput()			As Integer = {40700, 7886}	' 8		4						M_FLOAT
	Dim MODBUS_ModelID()				As Integer = {40800, 3505}	' 1		MODEL_SIZE				M_STRING
	Dim MODBUS_SerialNumber()			As Integer = {40900, 3521}	' 1		SERIAL_NUMBER_SIZE		M_FLOAT
	Dim MODBUS_CurrentTaskID()			As Integer = {41000, 3027}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_CurrentStateID()			As Integer = {41001, 3028}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_CommandResponse()		As Integer = {41002, 3029}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_InMenuState()			As Integer = {41003, 3050}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_ForceStopState()			As Integer = {41004, 3052}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_PreviousTaskID()			As Integer = {41005, 3079}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_PreviousLoopCount()		As Integer = {41006, 3095}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_StateDowncounter()		As Integer = {41007, 3096}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_CurrentLoopCount()		As Integer = {41008, 3097}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_MaxLoopCount()			As Integer = {41009, 3098}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_LastStateID()			As Integer = {41010, 3107}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_InputTaskCommand()		As Integer = {41011, 4026}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_CurrentIOStates()		As Integer = {41100, 3124}	'16		2						M_INTEGER_WORD
	Dim MODBUS_CurrentCommands()		As Integer = {41150, 3030}	'10		2						M_INTEGER_WORD
	Dim MODBUS_ThisStartTime()			As Integer = {41200, 6090}	' 1		4						M_INTEGER_DWORD
	Dim MODBUS_ThisEndTime()			As Integer = {41202, 6092}	' 1		4						M_INTEGER_DWORD
	Dim MODBUS_LastStartTime()			As Integer = {41204, 6108}	' 1		4						M_INTEGER_DWORD
	Dim MODBUS_LastEndTIme()			As Integer = {41206, 6110}	' 1		4						M_INTEGER_DWORD
	Dim MODBUS_SystemHealth()			As Integer = {41208, 6656}	' 1		4						M_INTEGER_DWORD
	Dim MODBUS_ActiveAlarmCount()		As Integer = {41210, 6858}	' 1		4						M_INTEGER_DWORD
	Dim MODBUS_ActiveMessageCount()		As Integer = {41212, 3858}	' 1		4						M_INTEGER_DWORD
	Dim MODBUS_ProcessParameters()		As Integer = {41300, 3536}  '60		2						M_INTEGER_WORD

																	' 8		sizeof(ScheduleEntry)	M_SCHEDULE
	Dim MODBUS_StartBlockEnabled()		As Integer = {41500, 3182}	' 1     2						M_INTEGER_WORD need to be enabled for block
	Dim MODBUS_StartBlockScheduleDays()	As Integer = {41504, 3186}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_StartBlockScheduleTime()	As Integer = {41505, 3187}	' 1		2						M_INTEGER_WORD

																	' 8		sizeof(ScheduleEntry)	M_SCHEDULE
	Dim MODBUS_StopEnabledRegister()    As Integer = {41506, 3188}  ' 1     2						M_INTEGER_WORD need to be enabled for block
	Dim MODBUS_StopBlockScheduleDays()	As Integer = {41510, 3192}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_StopBlockScheduleTime()	As Integer = {41511, 3193}	' 1		2						M_INTEGER_WORD

																	' 8		sizeof(ScheduleEntry)	M_SCHEDULE
	Dim MODBUS_PrecoatScheduleEnable()  As Integer = {41600, 3254}	' 1     2						M_INTEGER_WORD need to be enabled for schedule
	Dim MODBUS_PrecoatScheduleDays()	As Integer = {41604, 3258}  ' 1		2						M_INTEGER_WORD
	Dim MODBUS_PrecoatScheduleTime()	As Integer = {41605, 3259}  ' 1		2						M_INTEGER_WORD

																	' 8		sizeof(ScheduleEntry)	M_SCHEDULE
	Dim MODBUS_DrainScheduleEnable()    As Integer = {41700, 3206}	' 1     2						M_INTEGER_WORD need to be enabled for schedule
	Dim MODBUS_DrainScheduleDays()		As Integer = {41704, 3210}  ' 1		2						M_INTEGER_WORD
	Dim MODBUS_DrainScheduleTime()		As Integer = {41705, 3211}  ' 1		2						M_INTEGER_WORD

																	' 8		sizeof(ScheduleEntry)	M_SCHEDULE
	Dim MODBUS_BumpScheduleEnable()     As Integer = {41800, 3134}	' 1		2						M_INTEGER_WORD need to be enabled for schedule
	Dim MODBUS_BumpScheduleDays()		As Integer = {41804, 3138}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_BumpScheduleTime()		As Integer = {41805, 3139}  ' 1		2						M_INTEGER_WORD

	Dim MODBUS_DeltaPTrigger()			As Integer = {42000, 7432}	' 1		4						M_FLOAT
	Dim MODBUS_DeltaPTimeToBump()		As Integer = {42002, 3434}	' 1		4						M_INTEGER_DWORD
	Dim MODBUS_DeltaPBumpEnable()		As Integer = {42004, 3431}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_AnalogPressureTrigger()	As Integer = {42006, 7468}	' 1		4						M_FLOAT
	Dim MODBUS_SystemTime()				As Integer = {43000, 8000}	' 1		4						M_INTEGER_DWORD
	Dim MODBUS_IPAddress()				As Integer = {43001, 8010}	' 1		4						M_INTEGER_DWORD
	Dim MODBUS_TCPRecvPackets()			As Integer = {43002, 8003}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_TCPSentPackets()			As Integer = {43003, 8002}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_TCPDropPackets()			As Integer = {43004, 8004}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_TCPMemErr()				As Integer = {43005, 8005}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_TCPMiscErr()				As Integer = {43006, 8006}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_IPPHYBSR()		        As Integer = {43007, 8007}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_IsInDebug()				As Integer = {43008, 8012}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_LastDiskError()			As Integer = {43009, 8020}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_LastDiskErrorState()		As Integer = {43010, 8021}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_SPIFlashChipType()		As Integer = {43011, 8022}	' 1		2						M_INTEGER_WORD
	Dim MODBUS_TestModbus()				As Integer = {56789,   NK}	' 1		2						M_INTEGER_WORD
#End Region

#Region "TASK IDs"
	'													      R    D7
	Dim TASK_BadUpgradeError()				As Integer = {  285,  141}	' Did not copy all files when doing upgrade
	Dim TASK_BreakPoint()					As Integer = {   NK,  163}
	Dim TASK_BumpAuto()						As Integer = {  259,  114}	' Bump Auto mode
	Dim TASK_BumpCycle()					As Integer = {  258,  120}	' Bump Cycle model
	Dim TASK_BumpCycleFilter()				As Integer = {   NK,  130}	' Manual bump when in filter mode
	Dim TASK_BumpDrainFilter()				As Integer = {   NK,  145}
	Dim TASK_BumpSet()						As Integer = {   NK,  119}	' Bump Set mode
	Dim TASK_BumpSetFilter()				As Integer = {   NK,  121}	' Bump Set from Filter mode
	Dim TASK_BumpStandby()					As Integer = {   NK,  139}
	
	Dim TASK_CalibrateTimeToNTP()			As Integer = {  518,   NK}	' Calibrate RTC to NTP
	Dim TASK_CleanBasket()					As Integer = {  260,  115}	' Clean Basket mode
	Dim TASK_ClearAllAlarms()				As Integer = {  514,  162}	' Surrogate Modbus/TCP command to clear all alarms

	Dim TASK_DeltaPSensorError()			As Integer = {  280,  125}	' It is a sensor disconnect/out of range error
	Dim TASK_DeltaPSensorErrorNoWait()		As Integer = {  283,   NK}
	Dim TASK_DiagnosticFilter()				As Integer = {  269,  131}	' Diagnostic Filter operation
	Dim TASK_DisableHMI()					As Integer = {  512,  160}	' Surrogate Modbus/TCP command to disable the HMI input (except STOP)
	Dim TASK_Disconnected()					As Integer = {    0,    0}
	Dim TASK_DrainRinseAutoDrain()			As Integer = {  270,  134}	' Drain / Rinse mode - with Relay-Controlled Drain valve
	Dim TASK_DrainRinseManualDrain()		As Integer = {  261,  116}	' Drain / Rinse mode - with Manual Drain valve
	Dim TASK_DrainToIdle()					As Integer = {  265,  126}	' Abort out of Drain mode and back to idle
	Dim TASK_DryRunFilter()					As Integer = {  273,  147}	' Dry Run for Steve
	Dim TASK_DualD4Filter()					As Integer = {   NK,  148}
	Dim TASK_DumpMemory()					As Integer = {  515,   NK}	' Surrogate Modbus/TCP command to dump PLC memory to disk file
	Dim TASK_DumpStatistics()				As Integer = {  517,   NK}	' Surrogate Modbus/TCP command to append statistic info to file

	Dim TASK_EnableHMI()					As Integer = {  513,  161}	' Surrogate Modbus/TCP command to enable the HMI input

	Dim TASK_Filter()						As Integer = {  257,  113}	' Filter mode
	Dim TASK_FilterCloseRegen()				As Integer = {   NK,  133}
	Dim TASK_FilterOpenRegen()				As Integer = {   NK,  132}
	Dim TASK_FlexClean()					As Integer = {  264,  124}	' Flex Hose clean
	Dim TASK_FlexCleanNoPump()				As Integer = {  272,  146}	' Flex Hose clean
	Dim TASK_FlowSensorError()				As Integer = {  275,  149}	' Flow sensor error
	Dim TASK_FlowSensorErrorNoWait()		As Integer = {  284,   NK}

	Dim TASK_Idle()							As Integer = {  256,  112}	' Idle mode
	Dim TASK_IdleNoWait()					As Integer = {  274,   NK}	' Go into idle without waiting for valves to close
	Dim TASK_InTransition()					As Integer = {    1,    1}

	Dim TASK_MainDrain()					As Integer = {  266,  127}	' Do Main Drain automated process
	Dim TASK_MainTankError()				As Integer = {  277,  123}	' Pump confirm was not detected after we turned pump on
	Dim TASK_MainTankErrorNoWait()			As Integer = {  281,   NK}
	Dim TASK_Maintenance()					As Integer = {   NK,  137}	' Maintenance mode when DeltaP value is too high
	Dim TASK_MaintPostDrainRinse()			As Integer = {   NK,  138}	' Started in Maintenance mode and have done Drain/Rinse, but needs to validate

	Dim TASK_PoolLevelLowOffHighOff()		As Integer = {   NK,  144}
	Dim TASK_PoolLevelLowOnHighOff()		As Integer = {   NK,  143}
	Dim TASK_PoolLevelLowOnHighOn()			As Integer = {   NK,  142}
	Dim TASK_PrecoatFilter()				As Integer = {   NK,  118}	' Precoat Filter mode
	Dim TASK_PressureError()				As Integer = {  278,  122}	' Only used internally when we detect the pressure enable
	Dim TASK_PressureErrorNoWait()			As Integer = {  282,   NK}
	Dim TASK_PressureSensorError()			As Integer = {  279,  136}	' Pressure sensor error
	Dim TASK_PressureSensorErrorNoWait()	As Integer = {  286,   NK}

	Dim TASK_SignalBreakpoint()				As Integer = {  516,   NK}	' Surrogate Modbus/TCP command to generate a breakpoint in debug mode
	Dim TASK_Standby()						As Integer = {  263,  135}	' Standby mode (same as TASK_FilterOpenRegen)
	Dim TASK_StrainerCycle()				As Integer = {  268,  129}	' Do Strainer Cycle process
	Dim TASK_SystemStartup()				As Integer = {  271,  140}	' When system started up - used for logging
	Dim TASK_SystemVentValveCycle()			As Integer = {  267,  128}	' Do Precoat LVV automated cycle process

	Dim TASK_VacuumTransfer()				As Integer = {  262,  117}	' Vacuum Transfer mode
	Dim TASK_ValvePressureOverLimit()		As Integer = {  276,   NK}	' Influent or Effluent sensor is over limit so shut down filter
	Dim TASK_DeltapLimit()		            As Integer = {  276,   NK}	' Delta p has not been fixed after multiple bumps, shut down the filter
#End Region

#Region "STATE IDs"
	'																    R    D7
	Dim STATE_None()									As Integer = {  0,    0} '-	
	Dim STATE_SetNextStep()								As Integer = {  1,    1} '- Set next valid step after current one	
	Dim STATE_ErrorNoVacuum()							As Integer = {  2,    2} '- Error because we have no vacuum pressure	
	Dim STATE_ErrorDeltaPSensor()						As Integer = {  3,    3} '- Sensor disconnected error	
	Dim STATE_SetPreviousStep()							As Integer = {  4,    4} '- Set previous valid step before current one	
	Dim STATE_ErrorNoMainPump()							As Integer = {  5,    5} '- Error because main pump is not active	
	Dim STATE_ErrorPressureSensor()						As Integer = {  6,    6} '- Sensor disconnected error	
	Dim STATE_BadUpgradeError()							As Integer = {  7,    7} '- Bad upgrade error	
	Dim STATE_ErrorFlowSensor()							As Integer = {  8,    8} '-	
	Dim STATE_AuxCheckState()							As Integer = {  9,    9} '- Auxiliary check state with function callback	
	Dim STATE_ErrorValveOverLimit()						As Integer = { 10,   NK} '- Influent/Effluent valve over pressure limit	

	Dim STATE_ErrorBumpFail()						    As Integer = { 13,   NK} '- After looping through bumps, we have not corrected our Delta-p
																	   	   
	Dim STATE_Filter()									As Integer = { 16,   16} '- Filter Mode final state	
	Dim STATE_FilterStartup()							As Integer = { 17,   NK} '-	
	Dim STATE_FilterRegenStart()						As Integer = { 18,   17} '- Filter mode - step 1	
	Dim STATE_FilterPumpOn()							As Integer = { 19,   18} '- Filter mode - step 1b	
	Dim STATE_FilterOpenRegen()							As Integer = { 20,   19} '- Filter mode - step 2	
	Dim STATE_FilterPrecoat()							As Integer = { 21,   20} '- Filter mode - step 2b	
	Dim STATE_FilterOpenEffluent()						As Integer = { 22,   21} '- Filter mode - step 3	
	Dim STATE_FilterCloseRegen()						As Integer = { 23,   22} '- Filter mode - step 4	
	Dim STATE_FilterFiremanOn()							As Integer = { 24,   23} '- Filter mode - step 5	
	Dim STATE_FilterRegenOpen()							As Integer = { 25,   24} '- Filter Mode - special regen open state	
	Dim STATE_FilterCloseEffluent()						As Integer = { 26,   25} '- Filter/Standby mode - close effluent	
	Dim STATE_FilterDrainOpen()							As Integer = { 27,   26} '- Idle - Bump - Filter for Dual 	 open drain
	Dim STATE_FilterDrainClose()						As Integer = { 28,   27} '- Idle - Bump - Filter for Dual 	 close drain
	Dim STATE_FilterMainPumpOn()						As Integer = { 29,   28} '- Filter Mode - turn on Main Pump (if have Precoat pump option)	
	Dim STATE_FilterMainPumpOff()						As Integer = { 30,   29} '- Filter Mode - turn off Main Pump (if have Precoat pump option)	
	Dim STATE_FilterPrecoatPumpOff()					As Integer = { 31,   30} '- Filter Mode - turn off Main Pump (if have Precoat pump option)	
																	   		 
	Dim STATE_IdleStartup()								As Integer = { 48,   32} '- Startup phase of Idle state from reset	
	Dim STATE_IdleFilterStartup()						As Integer = { 49,   NK} '- Idle startup from Filter mode	
	Dim STATE_IdleStopPump()							As Integer = { 50,   33} '- Idle - Stop Pump	
	Dim STATE_IdleRaiseBump()							As Integer = { 51,   34} '- Idle - Raise Bump	
	Dim STATE_IdleCloseRegen()							As Integer = { 52,   35} '- Idle - Close Regen valve	
	Dim STATE_IdleCloseEffluent()						As Integer = { 53,   36} '- Idle - CLose Effluent vale	
	Dim STATE_Idle()									As Integer = { 54,   37} '- Idle - Full Idle state	
	Dim STATE_IdleStopFireman()							As Integer = { 55,   38} '- Idle - Time to delay after stopping Fireman	
	Dim STATE_IdleOpenRegen()							As Integer = { 56,   NK} '- Idle - Open Regen valve when going from Filter to Idle	
	Dim STATE_MaintenanceMode()							As Integer = { 57,   39} '- Maintenance Mode - requires Drain/Rinse to exit	
																	   		 
	Dim STATE_CleanCloseStrainer()						As Integer = { 64,   48} '- Clean - Close Strainer Valve	
	Dim STATE_CleanBasket()								As Integer = { 65,   49} '- Clean - Remove Basket	
	Dim STATE_CleanOpenStrainer()						As Integer = { 66,   50} '- Clean - Open Strainer Valve	
	Dim STATE_CleanStartBumpWait()						As Integer = { 67,   51} '- Clean - Wait for bump to start from user	
	Dim STATE_CleanStartPump()							As Integer = { 68,   52} '- Clean - Starting pump	
	Dim STATE_CleanStopPump()							As Integer = { 69,   53} '- Clean - Stopping pump	
	Dim STATE_CleanBumpOff()							As Integer = { 70,   54} '- Clean - Bump valve off (down)	
	Dim STATE_CleanBumpOn()								As Integer = { 71,   55} '- Clean - Bump valve on (up)	
	Dim STATE_MainDrainOn()								As Integer = { 72,   56} '- Main Drain Cycle - Main Drain On	
	Dim STATE_MainDrainOff()							As Integer = { 73,   57} '- Main Drain Cycle - Main Drain Off	
	Dim STATE_PLVVOn()									As Integer = { 74,   58} '- PLVV Cycle - PLVV On	
	Dim STATE_PLVVOff()									As Integer = { 75,   59} '- PLVV Cycle - PLVV Off	
	Dim STATE_StrainerFlip()							As Integer = { 76,   60} '- Strainer Cycle - Flip state	
																	   		 
	Dim STATE_DrainClosePumpThrottleValve()				As Integer = { 80,   64} '- Drain - Close Pump Discharge valve	
	Dim STATE_DrainStartBump()							As Integer = { 81,   65} '- Drain - Start Bump	
	Dim STATE_DrainBumpOff()							As Integer = { 82,   66} '- Drain - Bump Off	
	Dim STATE_DrainBumpOn()								As Integer = { 83,   67} '- Drain - Bump On	
	Dim STATE_DrainOpenDrain()							As Integer = { 84,   68} '- Drain - Open Drain Valve	
	Dim STATE_DrainCloseDrain()							As Integer = { 85,   69} '- Drain - Close Drain Valve	
	Dim STATE_DrainStartTransferSwitch()				As Integer = { 86,   70} '- Drain - Start Transfer Switch wait	
	Dim STATE_DrainTransferSwitchOn()					As Integer = { 87,   71} '- Drain - Actually turn Transfer Switch on	
	Dim STATE_DrainTransferSwitchOff()					As Integer = { 88,   72} '- Drain - Actually turn Transfer Switch off	
	Dim STATE_DrainOpenPumpThrottleValve()				As Integer = { 89,   73} '- Drain - Open pump discharge valve	
	Dim STATE_DrainStartPump()							As Integer = { 90,   74} '- Drain - Starting pump	
	Dim STATE_DrainStopPump()							As Integer = { 91,   75} '- Drain - Stop pump	
	Dim STATE_DrainBumpSelect()							As Integer = { 92,   76} '-	
	Dim STATE_DrainStartingPump()						As Integer = { 93,   77} '-	
	Dim STATE_DrainAutoBumpOff()						As Integer = { 94,   78} '-	
	Dim STATE_DrainAutoBumpOn()							As Integer = { 95,   79} '-	
	Dim STATE_DrainAutoBumpLoop()						As Integer = { 96,   80} '-	
	Dim STATE_DrainBumpSelectB()						As Integer = { 97,   81} '-	
	Dim STATE_RinseRepeat()								As Integer = { 98,   82} '-	
	Dim STATE_DrainToFilter()							As Integer = { 99,   83} '-	
	Dim STATE_DrainAutoBumpStart()						As Integer = {100,   84} '-	
	Dim STATE_DrainOpenDrainManual()					As Integer = {101,   85} '-	
	Dim STATE_DrainCloseDrainManual()					As Integer = {102,   86} '-	
	Dim STATE_DrainOpenDrainStart()						As Integer = {103,   87} '-	
	Dim STATE_DrainOpenDrainCheck()						As Integer = {104,   88} '-	
	Dim STATE_DrainCloseDrainAuto()						As Integer = {105,   89} '-	
	Dim STATE_DrainOpenDrainManual2()					As Integer = {106,   90} '-	
	Dim STATE_DrainCloseDrainManual2()					As Integer = {107,   91} '-	
	Dim STATE_DrainCloseDrainAuto2()					As Integer = {108,   92} '-	
	Dim STATE_DrainOpenDrainStart2()					As Integer = {109,   93} '-	
	Dim STATE_DrainOpenDrainCheck2()					As Integer = {110,   94} '-	
	Dim STATE_DrainCloseDrain2()						As Integer = {111,   95} '-	
	Dim STATE_DrainCloseTankVent()						As Integer = {112,   NK} '-	
	Dim STATE_DrainCloseSystemVentValveManualPOff()		As Integer = {113,   NK} '-	

	Dim STATE_VacuumCloseDrain()						As Integer = {128,   96} '-	
	Dim STATE_VacuumOpenTransferValve()					As Integer = {129,   97} '-	
	Dim STATE_VacuumOpenHoseValve()						As Integer = {130,   98} '-	
	Dim STATE_VacuumStartTransferSwitch()				As Integer = {131,   99} '-	
	Dim STATE_VacuumTransferSwitchOn()					As Integer = {132,  100} '-	
	Dim STATE_VacuumIntoFilter()						As Integer = {133,  101} '-	
	Dim STATE_VacuumTransferComplete()					As Integer = {134,  102} '-	
	Dim STATE_FillVacuumHoseSelect()					As Integer = {135,  103} '-	
	Dim STATE_DrainExitRepeat()							As Integer = {136,  104} '-	
	Dim STATE_DrainOpenDrain2()							As Integer = {137,  105} '-	
	Dim STATE_DrainRepeatLoopTop()						As Integer = {138,  106} '-	
	Dim STATE_DrainExitDrainLoop()						As Integer = {139,  107} '-	
	Dim STATE_DrainOpenTankVentManual2()				As Integer = {140,   NK} '-	
	Dim STATE_DrainOpenSystemVentValveManual2()			As Integer = {141,   NK} '-	

	Dim STATE_PrecoatCloseHoseValve()					As Integer = {144,  112} '-	
	Dim STATE_PrecoatCloseTransferValve()				As Integer = {145,  113} '-	
	Dim STATE_PrecoatOpenSystemVentValve()				As Integer = {146,  114} '-	
	Dim STATE_PrecoatOpenTankVentPartial()				As Integer = {147,  115} '-	
	Dim STATE_PrecoatOpenPumpThrottleValve()			As Integer = {148,  116} '-	
	Dim STATE_PrecoatStartPump()						As Integer = {149,  117} '-	
	Dim STATE_PrecoatCloseTankVent()					As Integer = {150,  118} '-	
	Dim STATE_PrecoatCloseSystemVentValve()				As Integer = {151,  119} '-	
	Dim STATE_PrecoatStopVacuumHose()					As Integer = {152,  120} '-	
	Dim STATE_PrecoatOpenTankVent()						As Integer = {153,  121} '-	
	Dim STATE_PrecoatStopPump()							As Integer = {154,  122} '-	
	Dim STATE_PrecoatStartingPump()						As Integer = {155,  123} '-	
	Dim STATE_PrecoatOpenSystemVentValveManual()		As Integer = {156,  124} '-	
	Dim STATE_PrecoatCloseSystemVentValveManual()		As Integer = {157,  125} '-	
	Dim STATE_PrecoatCloseTankVentPOff()				As Integer = {158,  126} '-	
	Dim STATE_PrecoatCloseSystemVentValveManualPOff()	As Integer = {159,  127} '-	
																		    
	Dim STATE_BumpSetStart()							As Integer = {176,  128} '-	
	Dim STATE_BumpSetBumpOff()							As Integer = {177,  129} '-	
	Dim STATE_BumpSetBumpOn()							As Integer = {178,  130} '-	
	Dim STATE_BumpSetLoop()								As Integer = {179,  131} '-	
	Dim STATE_BumpSelect()								As Integer = {180,  132} '-	
	Dim STATE_BumpAutoBumpOff()							As Integer = {181,  133} '-	
	Dim STATE_BumpAutoBumpOn()							As Integer = {182,  134} '-	
	Dim STATE_FlexCleanLoop()							As Integer = {183,  135} '-	
	Dim STATE_FlexCleanStartup()						As Integer = {184,  136} '-	
	Dim STATE_FlexStartPump()							As Integer = {185,  137} '-	
	Dim STATE_FlexStopPump()							As Integer = {186,  138} '-	
	Dim STATE_FlexWait()								As Integer = {187,  139} '-	
	Dim STATE_FlexOpenRegen()							As Integer = {188,  140} '-	
	Dim STATE_FlexPrecoat()								As Integer = {189,  141} '-	
	Dim STATE_FlexCloseRegen()							As Integer = {190,  142} '-	
	Dim STATE_BumpStartup()								As Integer = {191,   NK} '
	Dim STATE_FlexOpenTankVentManual()					As Integer = {192,   NK} '-	
	Dim STATE_FlexOpenVacuumHoseValve()					As Integer = {193,   NK} '-	
	Dim STATE_FlexLoadCleaner()							As Integer = {194,   NK} '-	
	Dim STATE_FlexCloseTankVent()						As Integer = {195,   NK} '-	
	Dim STATE_FlexCloseVacuumHoseValve()				As Integer = {196,   NK} '-	
	Dim STATE_FlexOpenPumpThrottleValve()				As Integer = {197,   NK} '-	
	Dim STATE_FlexBumpSetStart()						As Integer = {198,   NK} '-	
	Dim STATE_FlexBumpAutoBumpOff()						As Integer = {199,   NK} '-	
	Dim STATE_FlexBumpAutoBumpOn()						As Integer = {200,   NK} '-	
	Dim STATE_FlexBumpSetLoop()							As Integer = {201,   NK} '-	
	Dim STATE_FlexCloseDrainManual()					As Integer = {202,   NK} '-	
	Dim STATE_FlexCleanLoopTop()						As Integer = {203,   NK} '-	
	Dim STATE_FlexDrainStartBump()						As Integer = {204,   NK} '-	
	Dim STATE_FlexDrainAutoBumpStart()					As Integer = {205,   NK} '-	
	Dim STATE_FlexDrainAutoBumpOff()					As Integer = {206,   NK} '-	
	Dim STATE_FlexDrainAutoBumpOn()						As Integer = {207,   NK} '-	
	Dim STATE_FlexDrainAutoBumpLoop()					As Integer = {208,   NK} '-	
	Dim STATE_FlexDrainBumpSelectB()					As Integer = {209,   NK} '-	
	Dim STATE_FlexDrainClosePumpThrottleValve()			As Integer = {210,   NK} '-	
	Dim STATE_FlexDrainOpenDrainManual()				As Integer = {211,   NK} '-	
	Dim STATE_FlexDrainOpenTankVentManual()				As Integer = {212,   NK} '-	
	Dim STATE_FlexOpenSystemVentValveManual()			As Integer = {213,   NK} '-	
	Dim STATE_FlexDrainCloseDrainManual()				As Integer = {214,   NK} '-	
	Dim STATE_FlexDrainCloseRegen()						As Integer = {215,   NK} '-	
	Dim STATE_FlexStopPump2()							As Integer = {216,   NK} '-	

	Dim STATE_BumpAutoFiremanOn()						As Integer = {217,   NK} '-	
	Dim STATE_BumpAutoOpenRegen()						As Integer = {218,   NK} '-	
	Dim STATE_BumpAutoCloseEffluent()					As Integer = {219,   NK} '-	

	Dim STATE_DrainOpenTankVentManual()					As Integer = {224,  144} '-	
	Dim STATE_FillCloseDrain()							As Integer = {225,  145} '-	
	Dim STATE_FillCloseTankVent()						As Integer = {226,  146} '-	
	Dim STATE_FillOpenTransferValve()					As Integer = {227,  147} '-	
	Dim STATE_FillOpenV3Valve()							As Integer = {228,   NK} '-
	Dim STATE_FillStartVacuumHoseWait()					As Integer = {229,  148} '-	
	Dim STATE_FillStartVacuumHose()						As Integer = {230,  149} '-	
	Dim STATE_FillStopVacuumHose()						As Integer = {231,  150} '-	
	Dim STATE_DrainRegenStart()							As Integer = {232,  151} '-	
	Dim STATE_DrainPumpOn()								As Integer = {233,  152} '-	
	Dim STATE_DrainOpenRegen()							As Integer = {234,  153} '-	
	Dim STATE_DrainPrecoat()							As Integer = {235,  154} '-	
	Dim STATE_DrainFiremanOn()							As Integer = {236,  155} '-	
	Dim STATE_DrainOpenEffluent()						As Integer = {237,  156} '-	
	Dim STATE_DrainCloseRegen()							As Integer = {238,  157} '-	
	Dim STATE_FillOpenVacuumHoseValve()					As Integer = {239,  158} '-	
	Dim STATE_FillCloseVacuumHoseValve()				As Integer = {240,  159} '-	
	Dim STATE_FillCloseSystemVentValveManualPOff()		As Integer = {241,   NK} '-

	Dim STATE_IdleDrainProcessStop()					As Integer = {256,  160} '-	
	Dim STATE_IdleCloseDrain()							As Integer = {257,  161} '-	
	Dim STATE_IdleCloseTankVent()						As Integer = {258,  162} '-	
	Dim STATE_IdleStopVacuumHose()						As Integer = {259,  163} '-	
	Dim STATE_IdleCloseTransferValve()					As Integer = {260,  164} '-	
	Dim STATE_IdleCloseSystemVentValve()				As Integer = {261,  165} '-	
	Dim STATE_DrainToIdleStartup()						As Integer = {262,  166} '-	
	Dim STATE_IdleOpenPumpThrottleValve()				As Integer = {263,  167} '-	
	Dim STATE_IdleCloseDrainManual()					As Integer = {264,  168} '-	
	Dim STATE_IdleCloseSystemVentValveManual()			As Integer = {265,  169} '-	
	Dim STATE_DrainOpenTankVent()						As Integer = {266,  170} '-	

	Dim STATE_CouldNotReconnect()						As Integer = {65533, 65533} ' -
	Dim STATE_Reconnecting()							As Integer = {65534, 65534} ' -
	Dim STATE_Reseting()								As Integer = {65535, 65535} ' -

	'DEFENDER ADDED
	Dim STATE_WLS_HighOFFLowOFFFill()					As Integer = { NK, 4096} '-
	Dim STATE_WLS_HighOFFLowONFill()					As Integer = { NK, 4097} '-
	Dim STATE_WLS_HighONLowONFillInDelay()				As Integer = { NK, 4098} '-
	Dim STATE_WLS_HighONLowONNoFill()					As Integer = { NK, 4099} '-
	Dim STATE_WLS_InvalidStateError()					As Integer = { NK, 4100} '-
	Dim STATE_WLS_PumpShutoffLimitError()				As Integer = { NK, 4101} '-
	Dim STATE_WLS_None()								As Integer = { NK, 4102} '-
#End Region

#Region "PARAMETERS"
	'									               R    D7
	'											   41300  3536
	Dim DELAY_PUMP_OFF()				As Integer = { 0,   0}	' Wait time for pump to turn off
	Dim DELAY_PUMPRUN_CONFIRM_DELAY()	As Integer = { 1,  56}	' Wait time for pump to turn on
	Dim DELAY_BUMP_ON()					As Integer = { 2,   2}	' Wait time for Bump to go on
	Dim DELAY_BUMP_OFF()				As Integer = { 3,   3}	' Wait time for Bump to go off
	Dim DELAY_EFFLUENT_CLOSE()			As Integer = { 4,   4}	' Wait time for Effluent close process to complete
	Dim DELAY_EFFLUENT_OPEN()			As Integer = { 5,   5}	' Wait time for Effluent open process to complete
	Dim DELAY_REGEN_CLOSE()				As Integer = { 6,   6}	' Wait time for Regen close process to complete
	Dim DELAY_REGEN_OPEN()				As Integer = { 7,   7}	' Wait time for Regen open process to complete
	Dim DELAY_REGEN_START()				As Integer = { 8,   8}	' Wait time for Regen/Revival to start
	Dim DELAY_REGEN_VALVE()				As Integer = { 9,   9}	' Display Wait time for Regen/Revival to start
	Dim DELAY_DRYRUN_PRECOAT()			As Integer = {10,  13}	' Time used during dry run for precoat operation
	Dim DUAL_PRECOAT_PUMP_ENABLE()		As Integer = {11,  14}	' do we control both a recirc and a fill pump?
	Dim DELAY_BUMPSET_OFF()				As Integer = {12,  15}	' 
	Dim DELAY_BUMPSET_ON()				As Integer = {13,  16}	' 
	Dim DELAY_MAIN_PUMP_ON()			As Integer = {14,  17}	' NEW: How long to wait after turn on main pump
	Dim LOOPCOUNT_AUTOBUMP()			As Integer = {15,  18}	' Loop counter for Auto-Bump
	Dim DELAY_PRECOAT()					As Integer = {16,  19}	' Precoat wait after Regen valve is open
	Dim DELAY_BUMPAUTO_OFF()			As Integer = {17,  20}	' Manual Bump from front panel
	Dim DELAY_BUMPAUTO_ON()				As Integer = {18,  21}	' Manual Bump from front panel
	Dim LOOPCOUNT_FLEXBUMP()			As Integer = {19,  NK}	' 
	Dim DELAY_FIREMAN()					As Integer = {20,  23}	' 
	Dim TIME_SINCE_IDLE()				As Integer = {21,  24}	' How long stuck in Idle mode to generate alarm
	Dim TIME_SINCE_PRESSURE_OFF()		As Integer = {22,  25}	' How long with pressure off to generate alarm
	Dim TIME_SINCE_PROBE_ERROR()		As Integer = {23,  26}	' How long to wait for a probe to be in error before alarm
	Dim TIME_SINCE_BUMP()				As Integer = {24,  27}	' not used
	Dim TIME_SINCE_DRAINRINSE()			As Integer = {25,  28}	' How long is "too long since drain/rinse" when in filter mode before alarm?
	Dim DELAY_FIREMAN_OFF()				As Integer = {26,  30}	' How long to wait when doing Auto-bump after Fireman valve turned off?
	Dim DELAY_DIAGNOSTIC_DELAY()		As Integer = {27,  31}	' How long to wait for all diagnostic operations going from idle to filter
	Dim BUMP_PASSWORD_STATE()			As Integer = {28,  32}	' Should password for bumps be enabled?
	Dim DELAY_FLEXCLEAN_PUMP_ON()		As Integer = {29,  33}	' How long wait for pump to go on when doing Flex Clean
	Dim DELAY_PRECOAT_PUMP_OFF()		As Integer = {30,  34}	' NEW: How long to wait after close Regen valve before turn off Precoat Pump
	Dim DELAY_FLEXLOOP_DELAY()			As Integer = {31,  35}	' How long wait in between each Flex/Clean main loops
	Dim MAIN_DRAIN_OPTION_ENABLE()		As Integer = {32,  36}	' True if Main Drain automatic relay control is enabled
	Dim MAIN_DRAIN_ON_TIME()			As Integer = {33,  37}	' How long main drain should be on when scheduled to open
	Dim MAIN_DRAIN_CLOSE()				As Integer = {34,  34}	' How long main drain should be on when scheduled to open
	Dim PRECOAT_LVV_OPTION_ENABLE()		As Integer = {35,  39}	' True if Precoat Line Vent Valve automatic relay control is enabled
	Dim PRECOAT_LVV_CLOSE()				As Integer = {36,  40}	' How long Precoat Line Vent Valve should be on when scheduled to open
	Dim PRECOAT_LVV_ON_TIME()			As Integer = {37,  41}	' How long Precoat Line Vent Valve should be on when scheduled to open
	Dim CHAMBER_FAULT_SIZE()			As Integer = {38,  NK}	' size of pressure vessel
	Dim STRAINER_CYCLE_CYCLE_TIME()		As Integer = {39,  43}	' Time used to preload the strainer cycle downcounter
	Dim MAX_MENU_TIME()					As Integer = {40,  44}	' maximum amount of time to wait before exiting menu system
	Dim ANALOG_PRESSURE_ENABLE()		As Integer = {41,  47}	' Should we use Analog 
	Dim RUNTIME_INPUT_OVERRIDE()		As Integer = {42,  48}	' Should we runtime override the requirement to have relays? (Set by Modbus/TCP command)
	Dim VFD_ENABLE()					As Integer = {43,  52}	' Should we gather and display VFD values?
	Dim VFD_UNITS()						As Integer = {44,  53}	' GPM
	Dim DELAY_DELTAP_REVIVE_ENABLE()	As Integer = {45,  NK}	' Delta-P Revive Enable?
	Dim FLOW_ENABLE()					As Integer = {46,  57}	' Enable display/log of Flow Rate coming from Analog In #
	Dim FLOW_UNITS()					As Integer = {47,  58}	' Units to display
	Dim DELAY_FLEXCLEAN_OVERALL_TIME()	As Integer = {48,  NK}	' Overall time we should do pump/agitate cycle
	Dim ACTIVE_LANGUAGE()				As Integer = {49,  62}	' Which language should be used
	Dim MODBUSRTU_SLAVE_ID()			As Integer = {50,  NK}	' Modbus/RTU slave id
	Dim MODBUSRTU_SLAVE_BAUD_RATE()		As Integer = {51,  NK}	' Modbus/RTU slave baud rate
	Dim DELTAP_EMAIL_AFTER_BUMP_VALUE() As Integer = {52,  65}	' Value to use to send email after we've done bump
	Dim EMAIL_ERROR_DEFAULT_MASK()		As Integer = {53,  66}	' Mask of bits in sharedconf.h (EMAILERR_XXX) to default to
	Dim PRESSURE_UNITS()				As Integer = {54,  67}	' Pressure units
	Dim DELAY_DELTAP_DELAY_VALUE()		As Integer = {55,  NK}	' Delay time for Delta-P
	Dim BEEPER_ENABLE()					As Integer = {56,  NK}	' Enable the beeper when press touch screen
	Dim AUTO_RESTART_ENABLE()			As Integer = {57,  68}	' Auto-Restart option enable
	Dim SERVICE_MODE_ENABLE()			As Integer = {58,  NK}	' Service mode for Filter mode display
	Dim DELAY_DELTAP_ENABLE()			As Integer = {NK,  NK}	' Is DeltaP Revive enabled?
	Dim	DELAY_PUMP_ON()					As Integer = {NK,   1}
	Dim	DELAY_EFFLUENT_VALVE()			As Integer = {NK,  10}
	Dim	DELAY_BUMP_DOWN()				As Integer = {NK,  11}
	Dim	DELAY_BUMP_UP()					As Integer = {NK,  12}
	Dim	DELAY_VHOSE_VALVE()				As Integer = {NK,  22}
	Dim	TIME_SINCE_DIFF_HIGH()			As Integer = {NK,  29}
	Dim	IN_DRAIN_CLOSE()				As Integer = {NK,  38}
	Dim	STRAINER_CYCLE_OPTION_ENABLE()  As Integer = {NK,  42}
	Dim	DELAY_STRAINER_CLOSE()			As Integer = {NK,  45}
	Dim	DUAL_MODE_CONTROL()				As Integer = {NK,  46}
	Dim	WATER_LEVEL_ENABLE()			As Integer = {NK,  49}
	Dim	WATER_LEVEL_ON_HOLD_TIME()		As Integer = {NK,  50}
	Dim	WATER_LEVEL_STABILIZE_TIME()	As Integer = {NK,  51}
	Dim	DELAY_DUAL_DRAIN_OPEN()			As Integer = {NK,  54}
	Dim	WATER_LEVEL_MAX_PUMP_TIME()		As Integer = {NK,  55}
	Dim	LOG_FREQUENCY()					As Integer = {NK,  59}
	Dim	DUALD4_WATCHDOG_TIMEOUT_MIN()	As Integer = {NK,  60}
	Dim	DUALD4_STANDBY_EFFLUENT_OPEN()  As Integer = {NK,  61}
	Dim	PERLITE_REQUIRED_POUNDS()		As Integer = {NK,  63}
	Dim	PERLITE_REQUIRED_KILOGRAMS()	As Integer = {NK,  64}
	Dim	REMOTE_ONOFF_CONTROL_ENABLE()	As Integer = {NK,  69}
	Dim	ALARM_RELAY_OUTPUT_ENABLE()		As Integer = {NK,  70}

#End Region

#Region "OTHER"
	'									     R  D7
	Dim maxAnalogChannels()  As Integer = {  4,  4}
	Dim maxDigitalChannels() As Integer = {  3,  6}
	Dim maxParameters()      As Integer = { 60, 71} ' base index 1
	Dim maxCommands()        As Integer = { 12, 12}

	Dim DI_Stop()    		 As Integer = { NK,  1} ' base index 0
	Dim DI_PumpConfirmON()   As Integer = {  2,  2}
	Dim DI_Start()			 As Integer = { NK,  3}
	Dim DI_4() 				 As Integer = { NK,  4}

	Dim OnMask()			 As UShort = {1 ,  2,  4,  8, 16, 32}
	Dim OffMask()			 As UShort = {62, 61, 59, 55, 47, 31}

	Dim password()			 As String = {"clearblue", "belleville"}
	Dim resetString()		 As String = {"/run_json?action=restart.json&restart=yes","/process_json?name=restart.json&restart=yes"}

	Dim systemIndex = 0
#End Region


#Region "DAY OF THE WEEK BITMASK"
	Const SUNDAY As Integer = 1
	Const MONDAY As Integer = 2
	Const TUESDAY As Integer = 4
	Const WEDNESDAY As Integer = 8
	Const THURSDAY As Integer = 16
	Const FRIDAY As Integer = 32
	Const SATURDAY As Integer = 64
#End Region

#Region "SCRIPT DEFINED VARIABLES"
	Public Structure varTableEntry
		Public name As String
		Public value As Integer
	End Structure

	Dim vTable(20) As varTableEntry
	Dim lastTableIndex As Integer = 0
#End Region

#Region "OTHER VARIABLES"
	Const INDEX_REBIG As Integer = 0
	Const INDEX_D7    As Integer = 1
	Const INDEX_2300  As Integer = 2

	public Const MODEL_REBIG As String = "Aqua Revival"
	public Const MODEL_D7    As String = "Defender7"
	public Const MODEL_2300  As String = "AquaMetrix 2300"

	Const HOURS_IN_DAY       As Integer = 24

	Const MINUTES_IN_HOUR    As Integer = 60
	Const MINUTES_IN_DAY     As Integer = HOURS_IN_DAY * MINUTES_IN_HOUR

	Const SECONDS_IN_MINUTES As Integer = 60
	Const SECONDS_IN_HOUR    As Integer = SECONDS_IN_MINUTES * MINUTES_IN_HOUR
	Const SECONDS_IN_DAY     As Integer = SECONDS_IN_HOUR * HOURS_IN_DAY

	Const MILLISECONDS_IN_SECONDS As Integer = 1000

	'Dim sqlapi                  As New SQL_API()
	Dim CurrentState            As UShort = 0
	Dim thisServerInfo          As ThreadComm
	Dim thisCurrentLineTextBox  As TextBox
	Dim thisLoopCountTextbox    As TextBox
	Dim thisErrorCountTextbox   As TextBox
	Dim thisLastTimeTextbox     As TextBox
	Dim thisCurrentStateTextbox As TextBox
	Dim thisCurrentTestTextbox  As TextBox
	Dim thisIndicatorTextBox    As TextBox
	Dim thisIndicatorTTextBox   As TextBox
	Dim thisModelLabel			As Label
	Dim thisIPTextBox			As TextBox
	Dim thisIsFixedIP			As Boolean

	Dim thisSerialNumber        As String = ""
	Dim thisCPUVersion          As String = ""
	Dim thisModel				As String = ""
	Dim CurrentLine             As String = ""
	Dim LoopCount               As Integer = 0
	Dim currentTest             As String = ""

	Dim errorCount              As Integer = 0
	Dim diginValues             As Integer = 0
	Dim logHandle               As StreamWriter
#End Region

	Public Sub New(ByRef ServerInfo As ThreadComm, ByRef CurrentLineTextBox As TextBox, ByRef ErrorCountTextbox As TextBox, ByRef LoopCountTextbox As TextBox, 
				   ByRef LastTimeTextbox As TextBox, ByRef CurrentStateTextbox As TextBox, ByVal ExitCommandState As Integer, ByRef CurrentTestTextbox As TextBox, 
				   byref IndicatorTextbox As TextBox, byref IndicatorTTextbox As TextBox, byref ModelLabel As Label, byref IPTextBox As TextBox, byRef isFixedIP As boolean)
		thisServerInfo          = ServerInfo

		thisCurrentLineTextBox  = CurrentLineTextBox
		thisLoopCountTextbox    = LoopCountTextbox
		thisErrorCountTextbox   = ErrorCountTextbox
		thisLastTimeTextbox     = LastTimeTextbox
		thisCurrentStateTextbox = CurrentStateTextbox
		thisCurrentTestTextbox  = CurrentTestTextbox
		thisIndicatorTextBox    = IndicatorTextbox
		thisIndicatorTTextBox   = IndicatorTTextbox
		thisModelLabel			= ModelLabel
		thisIPTextBox			= IPTextBox
		thisIsFixedIP			= isFixedIP
	End Sub

#Region "UPDATE UI SUBs"
	Private Sub UpdateGUI()
		DisplayCurrentLine()
		DisplayErrorCount()
		DisplayLoopCount()
		DisplayLastTime()
		DisplayCurrentState()
	End Sub

	Private Delegate Sub DisplayCurrentLineDelegate()
	Private Sub DisplayCurrentLine()
		If thisServerInfo.ExitCommand = 1 Then
			Return
		End If
		If thisCurrentLineTextBox.InvokeRequired Then
			thisCurrentLineTextBox.BeginInvoke(New DisplayCurrentLineDelegate(AddressOf DisplayCurrentLine))
		Else
			If thisServerInfo.ExitCommand = 1 Then
				thisCurrentLineTextBox.Clear()
			Else
				thisCurrentLineTextBox.Text = CurrentLine
			End If
		End If
	End Sub

	Private Delegate Sub DisplayLoopCountDelegate()
	Private Sub DisplayLoopCount()
		If thisServerInfo.ExitCommand = 1 Then
			Return
		End If
		If thisLoopCountTextbox.InvokeRequired Then
			thisLoopCountTextbox.BeginInvoke(New DisplayLoopCountDelegate(AddressOf DisplayLoopCount))
		Else
			If thisServerInfo.ExitCommand = 1 Then
				thisLoopCountTextbox.Clear()
			Else
				thisLoopCountTextbox.Text = LoopCount.ToString()
			End If
		End If
	End Sub

	Private Delegate Sub DisplayErrorCountDelegate()
	Private Sub DisplayErrorCount()
		If thisServerInfo.ExitCommand = 1 Then
			Return
		End If
		If thisErrorCountTextbox.InvokeRequired Then
			thisErrorCountTextbox.BeginInvoke(New DisplayErrorCountDelegate(AddressOf DisplayErrorCount))
		Else
			If thisServerInfo.ExitCommand = 1 Then
				thisErrorCountTextbox.Clear()
			Else
				thisErrorCountTextbox.Text = errorCount.ToString()
			End If
		End If
	End Sub

	Private Delegate Sub DisplayLastTimeDelegate()
	Private Sub DisplayLastTime()
		If thisServerInfo.ExitCommand = 1 Then
			Return
		End If
		If thisLastTimeTextbox.InvokeRequired Then
			thisLastTimeTextbox.BeginInvoke(New DisplayLastTimeDelegate(AddressOf DisplayLastTime))
		Else
			If thisServerInfo.ExitCommand = 1 Then
				thisLastTimeTextbox.Clear()
			Else
				thisLastTimeTextbox.Text = Date.Now().ToString("G")
			End If
		End If
	End Sub

	Private Delegate Sub DisplayCurrentStateDelegate()
	Private Sub DisplayCurrentState()
		If thisServerInfo.ExitCommand = 1 Then
			Return
		End If
		If thisCurrentStateTextbox.InvokeRequired Then
			Try
				thisCurrentStateTextbox.BeginInvoke(New DisplayCurrentStateDelegate(AddressOf DisplayCurrentState))
			Catch ex As Exception
				MsgBox(ex.Message)
			End Try
			
		Else
			If thisServerInfo.ExitCommand = 1 Then
				thisCurrentStateTextbox.Clear()
			Else
				Dim newText As String = GetStateStringFromStateInteger(CurrentState)

				thisCurrentStateTextbox.Text = newText
			End If
		End If
	End Sub

	Private Delegate Sub DisplayCurrentTestDelegate()
	Private Sub DisplayCurrentTest()
		If thisServerInfo.ExitCommand = 1 Then
			Return
		End If
		If thisCurrentTestTextbox.InvokeRequired Then
			thisCurrentTestTextbox.BeginInvoke(New DisplayCurrentTestDelegate(AddressOf DisplayCurrentTest))
		Else
			If thisServerInfo.ExitCommand = 1 Then
				thisCurrentTestTextbox.Clear()
			Else
				thisCurrentTestTextbox.Text = currentTest.ToString()
			End If
		End If
	End Sub

	Private Delegate Sub DisplayIndicatorDelegate()
	Private Sub DisplayIndicator()
		If thisServerInfo.ExitCommand = 1 Then
			Return
		End If
		If thisIndicatorTextBox.InvokeRequired Then
			thisIndicatorTextBox.BeginInvoke(New DisplayIndicatorDelegate(AddressOf DisplayIndicator))
		Else
			If thisServerInfo.ExitCommand = 1 Then
				thisIndicatorTextBox.BackColor = Color.Gray
			Else
				thisIndicatorTextBox.BackColor = Color.Red
			End If
		End If
	End Sub

	Private Delegate Sub DisplayIndicatorTDelegate()
	Private Sub DisplayIndicatorT()
		If thisServerInfo.ExitCommand = 1 Then
			Return
		End If
		If thisIndicatorTTextBox.InvokeRequired Then
			thisIndicatorTTextBox.BeginInvoke(New DisplayIndicatorTDelegate(AddressOf DisplayIndicatorT))
		Else
			If thisServerInfo.ExitCommand = 1 Then
				thisIndicatorTTextBox.BackColor = Color.Gray
			Else
				thisIndicatorTTextBox.BackColor = Color.Red
			End If
		End If
	End Sub

	Private Delegate Sub DisplayModelDelegate()
	Private Sub DisplayModel()
		If thisServerInfo.ExitCommand = 1 Then
			Return
		End If
		If thisModelLabel.InvokeRequired Then
			thisModelLabel.BeginInvoke(New DisplayModelDelegate(AddressOf DisplayModel))
		Else
			If thisServerInfo.ExitCommand = 1 Then
				thisModelLabel.Text = ""
			Else
				thisModelLabel.Text = thisModel & ":" & thisSerialNumber
			End If
		End If
	End Sub

	Private Delegate Sub DisplayIPDelegate()
	Private Sub DisplayIP()
		If thisServerInfo.ExitCommand = 1 Then
			Return
		End If
		If thisIPTextBox.InvokeRequired Then
			thisIPTextBox.BeginInvoke(New DisplayIPDelegate(AddressOf DisplayIP))
		Else
			If thisServerInfo.ExitCommand = 1 Then
				thisIPTextBox.Text = ""
			Else
				thisIPTextBox.Text = thisServerInfo.ServerName
			End If
		End If
	End Sub
#End Region

#Region "CONVERT TASK <-> VALUE"
	' given a TASK value, return a corresponding string
	Private Function GetTaskStringFromTaskInteger(ByVal Task As UShort) As String
		Dim result As String = ""

		Select Case Task
			Case TASK_BadUpgradeError(systemIndex)
				result = "Bad Upgrade Error"

			Case TASK_BreakPoint(systemIndex)
				result = "Break Point"

			Case TASK_BumpAuto(systemIndex)
				result = "Bump Auto"

			Case TASK_BumpCycle(systemIndex)
				result = "Bump Cycle"

			Case TASK_BumpCycleFilter(systemIndex)
				result = "Bump Cycle Filter"

			Case TASK_BumpDrainFilter(systemIndex)
				result = "Bump Drain Filter"

			Case TASK_BumpSet(systemIndex)
				result = "Bump Set"

			Case TASK_BumpSetFilter(systemIndex)
				result = "Bump Set Filter"

			Case TASK_BumpStandby(systemIndex)
				result = "Bump Standby"



			Case TASK_CalibrateTimeToNTP(systemIndex)
				result = "Calibrate Time To NTP"

			Case TASK_CleanBasket(systemIndex)
				result = "Clean Basket"

			Case TASK_ClearAllAlarms(systemIndex)
				result = "Clear All ALarms"



			Case TASK_DeltaPSensorError(systemIndex)
				result = "Delta P Sensor Error"

			Case TASK_DeltaPSensorErrorNoWait(systemIndex)
				result = "Delta P Sensor Error No Wait"

			Case TASK_DiagnosticFilter(systemIndex)
				result = "Diagnostic Filter"

			Case TASK_DisableHMI(systemIndex)
				result = "Disable HMI"

			Case TASK_Disconnected(systemIndex)
				result = "Disconnected"

			Case TASK_DrainRinseAutoDrain(systemIndex)
				result = "Drain Rinse Auto Drain"

			Case TASK_DrainRinseManualDrain(systemIndex)
				result = "Drain Rinse Manual Drain"

			Case TASK_DrainToIdle(systemIndex)
				result = "Drain to Idle"

			Case TASK_DryRunFilter(systemIndex)
				result = "Dry Run Filter"

			Case TASK_DualD4Filter(systemIndex)
				result = "Dual D4 Filter"

			Case TASK_DumpMemory(systemIndex)
				result = "Dump Memory"

			Case TASK_DumpStatistics(systemIndex)
				result = "Dump Statistics"



			Case TASK_EnableHMI(systemIndex)
				result = "Enable HMI"



			Case TASK_Filter(systemIndex)
				result = "Filter"

			Case TASK_FilterCloseRegen(systemIndex)
				result = "Filter Close Regen"

			Case TASK_FilterOpenRegen(systemIndex)
				result = "Filter Open Regen"

			Case TASK_FlexClean(systemIndex)
				result = "Flex Clean"

			Case TASK_FlexCleanNoPump(systemIndex)
				result = "Flex Clean No Pump"

			Case TASK_FlowSensorError(systemIndex)
				result = "Flow Sensor Error"

			Case TASK_FlowSensorErrorNoWait(systemIndex)
				result = "Flow Sensor Error No Wait"



			Case TASK_Idle(systemIndex)
				result = "Idle"

			Case TASK_IdleNoWait(systemIndex)
				result = "Idle No Wait"

			Case TASK_InTransition(systemIndex)
				result = "In Transistion"



			Case TASK_MainDrain(systemIndex)
				result = "Main Drain"

			Case TASK_MainTankError(systemIndex)
				result = "Main Tank Error"

			Case TASK_MainTankErrorNoWait(systemIndex)
				result = "Main Tank Error No Wait"

			Case TASK_Maintenance(systemIndex)
				result = "Maintenance"

			Case TASK_MaintPostDrainRinse(systemIndex)
				result = "Maint Post Drain Rinse"



			Case TASK_PoolLevelLowOffHighOff(systemIndex)
				result = "Pool Level Low Off High Off"

			Case TASK_PoolLevelLowOnHighOff(systemIndex)
				result = "Pool Level Low On High Off"

			Case TASK_PoolLevelLowOnHighOn(systemIndex)
				result = "Pool Level Low On High On"

			Case TASK_PrecoatFilter(systemIndex)
				result = "Precoat Filter"

			Case TASK_PressureError(systemIndex)
				result = "Pressure Error"

			Case TASK_PressureErrorNoWait(systemIndex)
				result = "Pressure Error No Wait"

			Case TASK_PressureSensorError(systemIndex)
				result = "Pressure Sensor Error"

			Case TASK_PressureSensorErrorNoWait(systemIndex)
				result = "Pressure Sensor Error No Wait"



			Case TASK_SignalBreakpoint(systemIndex)
				result = "Signal Breakpoint"

			Case TASK_Standby(systemIndex)
				result = "Standby"

			Case TASK_StrainerCycle(systemIndex)
				result = "Strainer Cycle"

			Case TASK_SystemStartup(systemIndex)
				result = "System Start Up"

			Case TASK_SystemVentValveCycle(systemIndex)
				result = "System Vent Valve Cycle"



			Case TASK_VacuumTransfer(systemIndex)
				result = "Vaccum Transfer"

			Case TASK_ValvePressureOverLimit(systemIndex)
				result = "Valve Pressure Over Limit"

			Case TASK_DeltapLimit(systemIndex)
				result = "Delta-p Not Resolved"
				
			Case Else
				result = "Unknown (0x" & Task.ToString("X") & ")"
		End Select

		Return result
	End Function

	' given a TASK string, return a corresponding value
	Private Function GetTaskIntegerFromTaskString(ByRef task As String) As UShort
		Dim result As UShort = 0

		If task = "Bad_Upgrade_Error"
			result = TASK_BadUpgradeError(systemIndex)

		Elseif task = "Break_Point"
			result = TASK_BreakPoint(systemIndex)

		Elseif task = "Bump_Auto"
			result = TASK_BumpAuto(systemIndex)

		Elseif task = "Bump_Cycle"
			result = TASK_BumpCycle(systemIndex)

		Elseif task = "Bump_Cycle_Filter"
			result = TASK_BumpCycleFilter(systemIndex)

		Elseif task = "Bump_Drain_Filter"
			result = TASK_BumpDrainFilter(systemIndex)

		Elseif task = "Bump_Set"
			result = TASK_BumpSet(systemIndex)

		Elseif task = "Bump_Set_Filter"
			result = TASK_BumpSetFilter(systemIndex)

		Elseif task = "Bump_Standby"
			result = TASK_BumpStandby(systemIndex)



		Elseif task = "Calibrate_Time_To_NTP"
			result = TASK_CalibrateTimeToNTP(systemIndex)

		Elseif task = "Clean_Basket"
			result = TASK_CleanBasket(systemIndex)

		Elseif task = "Clear_All_ALarms"
			result = TASK_ClearAllAlarms(systemIndex)



		Elseif task = "Delta_P_Sensor_Error"
			result = TASK_DeltaPSensorError(systemIndex)

		Elseif task = "Delta_P_Sensor_Error_No_Wait"
			result = TASK_DeltaPSensorErrorNoWait(systemIndex)

		Elseif task = "Diagnostic_Filter"
			result = TASK_DiagnosticFilter(systemIndex)

		Elseif task = "Disable_HMI"
			result = TASK_DisableHMI(systemIndex)

		Elseif task = "Disconnected"
			result = TASK_Disconnected(systemIndex)

		Elseif task = "Drain_Rinse_Auto_Drain"
			result = TASK_DrainRinseAutoDrain(systemIndex)

		Elseif task = "Drain_Rinse_Manual_Drain"
			result = TASK_DrainRinseManualDrain(systemIndex)

		Elseif task = "Drain_to_Idle"
			result = TASK_DrainToIdle(systemIndex)

		Elseif task = "Dry_Run_Filter"
			result = TASK_DryRunFilter(systemIndex)

		Elseif task = "Dual_D4_Filter"
			result = TASK_DualD4Filter(systemIndex)

		Elseif task = "Dump_Memory"
			result = TASK_DumpMemory(systemIndex)

		Elseif task = "Dump_Statistics"
			result = TASK_DumpStatistics(systemIndex)



		Elseif task = "Enable_HMI"
			result = TASK_EnableHMI(systemIndex)


		
		Elseif task = "Filter"
			result = TASK_Filter(systemIndex)

		Elseif task = "Filter_Close_Regen"
			result = TASK_FilterCloseRegen(systemIndex)

		Elseif task = "Filter_Open_Regen"
			result = TASK_FilterOpenRegen(systemIndex)

		Elseif task = "Flex_Clean"
			result = TASK_FlexClean(systemIndex)

		Elseif task = "Flex_Clean_No_Pump"
			result = TASK_FlexCleanNoPump(systemIndex)

		Elseif task = "Flow_Sensor_Error"
			result = TASK_FlowSensorError(systemIndex)

		Elseif task = "Flow_Sensor_Error_No_Wait"
			result = TASK_FlowSensorErrorNoWait(systemIndex)



		Elseif task = "Idle"
			result = TASK_Idle(systemIndex)

		Elseif task = "Idle_No_Wait"
			result = TASK_IdleNoWait(systemIndex)

		Elseif task = "In_Transistion"
			result = TASK_InTransition(systemIndex)



		Elseif task = "Main_Drain"
			result = TASK_MainDrain(systemIndex)

		Elseif task = "Main_Tank_Error"
			result = TASK_MainTankError(systemIndex)

		Elseif task = "Main_Tank_Error_No_Wait"
			result = TASK_MainTankErrorNoWait(systemIndex)

		Elseif task = "Maintenance"
			result = TASK_Maintenance(systemIndex)

		Elseif task = "Maint_Post_Drain_Rinse"
			result = TASK_MaintPostDrainRinse(systemIndex)



		Elseif task = "Pool_Level_Low_Off_High_Off"
			result = TASK_PoolLevelLowOffHighOff(systemIndex)

		Elseif task = "Pool_Level_Low_On_High_Off"
			result = TASK_PoolLevelLowOnHighOff(systemIndex)

		Elseif task = "Pool_Level_Low_On_High_On"
			result = TASK_PoolLevelLowOnHighOn(systemIndex)

		Elseif task = "Precoat_Filter"
			result = TASK_PrecoatFilter(systemIndex)

		Elseif task = "Pressure_Error"
			result = TASK_PressureError(systemIndex)

		Elseif task = "Pressure_Error_No_Wait"
			result = TASK_PressureErrorNoWait(systemIndex)

		Elseif task = "Pressure_Sensor_Error"
			result = TASK_PressureSensorError(systemIndex)

		Elseif task = "Pressure_Sensor_Error_No_Wait"
			result = TASK_PressureSensorErrorNoWait(systemIndex)



		Elseif task = "Signal_Breakpoint"
			result = TASK_SignalBreakpoint(systemIndex)

		Elseif task = "Standby"
			result = TASK_Standby(systemIndex)

		Elseif task = "Strainer_Cycle"
			result = TASK_StrainerCycle(systemIndex)

		Elseif task = "System_Start_Up"
			result = TASK_SystemStartup(systemIndex)

		Elseif task = "System_Vent_Valve_Cycle"
			result = TASK_SystemVentValveCycle(systemIndex)



		Elseif task = "Vaccum_Transfer"
			result = TASK_VacuumTransfer(systemIndex)

		Elseif task = "Valve_Pressure_Over_Limit"
			result = TASK_ValvePressureOverLimit(systemIndex)

		Elseif task = "Delta-p_Not_Resolved"
			result = TASK_DeltapLimit(systemIndex)
		
		Else
			' leave the value as it is.
			result = STATE_None(systemIndex)
		End If

		Return result
	End Function
#End Region

#Region "CONVERT STATE <-> VALUE"
	' given a State value, return a corresponding string
	Private Function GetStateStringFromStateInteger(ByVal State As UShort) As String
		Dim result As String = ""

		Select Case State
			Case STATE_None(systemIndex)
				result = "None"

			Case STATE_SetNextStep(systemIndex)
				result = "Set Next Step"

			Case STATE_ErrorNoVacuum(systemIndex)
				result = "Error No Vacuum"

			Case STATE_ErrorDeltaPSensor(systemIndex)
				result = "Error Delta P Sensor"

			Case STATE_SetPreviousStep(systemIndex)
				result = "Set Previous Step"

			Case STATE_ErrorNoMainPump(systemIndex)
				result = "Error No Main Pump"

			Case STATE_ErrorPressureSensor(systemIndex)
				result = "Error Pressure Sensor"

			Case STATE_BadUpgradeError(systemIndex)
				result = "Error Bad Upgrade"

			Case STATE_ErrorFlowSensor(systemIndex)
				result = "Error Flow Sensor"

			Case STATE_AuxCheckState(systemIndex)
				result = "Aux Check State"

			Case STATE_ErrorValveOverLimit(systemIndex)
				result = "Error Valve Over Limit"

			Case STATE_ErrorBumpFail(systemIndex)
				result = "Delta-p_Not_Resolved"


				'
				' 

			Case STATE_Filter(systemIndex)
				result = "Filter"

			Case STATE_FilterStartup(systemIndex)
				result = "Filter Startup"

			Case STATE_FilterRegenStart(systemIndex)
				result = "Filter Regen Start"

			Case STATE_FilterPumpOn(systemIndex)
				result = "Filter Pump On"

			Case STATE_FilterOpenRegen(systemIndex)
				result = "Filter Open Regen"

			Case STATE_FilterPrecoat(systemIndex)
				result = "Filter Precoat"

			Case STATE_FilterOpenEffluent(systemIndex)
				result = "Filter Open Effluent"

			Case STATE_FilterCloseRegen(systemIndex)
				result = "Filter Close Regen"

			Case STATE_FilterFiremanOn(systemIndex)
				result = "Filter Fireman On"

			Case STATE_FilterRegenOpen(systemIndex)
				result = "Filter Regen Open"

			Case STATE_FilterCloseEffluent(systemIndex)
				result = "Filter Close Effluent"

			Case STATE_FilterDrainOpen(systemIndex)
				result = "Filter Drain Open"

			Case STATE_FilterDrainClose(systemIndex)
				result = "Filter Drain Close"

			Case STATE_FilterMainPumpOn(systemIndex)
				result = "Filter Main Pump On"

			Case STATE_FilterMainPumpOff(systemIndex)
				result = "Filter Main Pump Off"

			Case STATE_FilterPrecoatPumpOff(systemIndex)
				result = "Filter Precoat Pump Off"

				'
				'

			Case STATE_IdleStartup(systemIndex)
				result = "Idle Startup"

			Case STATE_IdleFilterStartup(systemIndex)
				result = "Idle Filter Startup"

			Case STATE_IdleStopPump(systemIndex)
				result = "Idle Stop Pump"

			Case STATE_IdleRaiseBump(systemIndex)
				result = "Idle Raise Bump"

			Case STATE_IdleCloseRegen(systemIndex)
				result = "Idle Close Regen"

			Case STATE_IdleCloseEffluent(systemIndex)
				result = "Idle Close Effluent"

			Case STATE_Idle(systemIndex)
				result = "Idle"

			Case STATE_IdleStopFireman(systemIndex)
				result = "Idle Stop Fireman"

			Case STATE_IdleOpenRegen(systemIndex)
				result = "Idle Open Regen"

			Case STATE_MaintenanceMode(systemIndex)
				result = "Maintenance Mode"

				'
				' 

			Case STATE_CleanCloseStrainer(systemIndex)
				result = "Clean Close Strainer"

			Case STATE_CleanBasket(systemIndex)
				result = "Clean Basket"

			Case STATE_CleanOpenStrainer(systemIndex)
				result = "Clean Open Strainer"

			Case STATE_CleanStartBumpWait(systemIndex)
				result = "Clean Start Bump Wait"

			Case STATE_CleanStartPump(systemIndex)
				result = "Clean Start Pump"

			Case STATE_CleanStopPump(systemIndex)
				result = "Clean Stop Pump"

			Case STATE_CleanBumpOff(systemIndex)
				result = "Clean Bump Off"

			Case STATE_CleanBumpOn(systemIndex)
				result = "Clean Bump On"

			Case STATE_MainDrainOn(systemIndex)
				result = "Main Drain On"

			Case STATE_MainDrainOff(systemIndex)
				result = "Main Drain Off"

			Case STATE_PLVVOn(systemIndex)
				result = "PLVV On"

			Case STATE_PLVVOff(systemIndex)
				result = "PLVV Off"

			Case STATE_StrainerFlip(systemIndex)
				result = "Strainer Flip"

				'
				'

			Case STATE_DrainClosePumpThrottleValve(systemIndex)
				result = "Drain Close Pump Throttle Valve"

			Case STATE_DrainStartBump(systemIndex)
				result = "Drain Start Bump"

			Case STATE_DrainBumpOff(systemIndex)
				result = "Drain Bump Off"

			Case STATE_DrainBumpOn(systemIndex)
				result = "Drain Bump On"

			Case STATE_DrainOpenDrain(systemIndex)
				result = "Drain Open Drain"

			Case STATE_DrainCloseDrain(systemIndex)
				result = "Drain Close Drain"

			Case STATE_DrainStartTransferSwitch(systemIndex)
				result = "Drain Start Transfer Switch"

			Case STATE_DrainTransferSwitchOn(systemIndex)
				result = "Drain Transfer Switch On"

			Case STATE_DrainTransferSwitchOff(systemIndex)
				result = "Drain Transfer Switch Off"

			Case STATE_DrainOpenPumpThrottleValve(systemIndex)
				result = "Drain Open Pump Throttle Valve"

			Case STATE_DrainStartPump(systemIndex)
				result = "Drain Start Pump"

			Case STATE_DrainStopPump(systemIndex)
				result = "Drain Stop Pump"

			Case STATE_DrainBumpSelect(systemIndex)
				result = "Drain Bump Select"

			Case STATE_DrainStartingPump(systemIndex)
				result = "Drain Starting Pump"

			Case STATE_DrainAutoBumpOff(systemIndex)
				result = "Drain Auto Bump Off"

			Case STATE_DrainAutoBumpOn(systemIndex)
				result = "Drain Auto Bump On"

			Case STATE_DrainAutoBumpLoop(systemIndex)
				result = "Drain Auto Bump Loop"

			Case STATE_DrainBumpSelectB(systemIndex)
				result = "Drain Bump Select B"

			Case STATE_RinseRepeat(systemIndex)
				result = "Rinse Repeat"

			Case STATE_DrainToFilter(systemIndex)
				result = "Drain To Filter"

			Case STATE_DrainAutoBumpStart(systemIndex)
				result = "Drain Auto Bump Start"

			Case STATE_DrainOpenDrainManual(systemIndex)
				result = "Drain Open Drain Manual"

			Case STATE_DrainCloseDrainManual(systemIndex)
				result = "Drain Close Drain Manual"

			Case STATE_DrainOpenDrainStart(systemIndex)
				result = "Drain Open Drain Start"

			Case STATE_DrainOpenDrainCheck(systemIndex)
				result = "Drain Open Drain Check"

			Case STATE_DrainCloseDrainAuto(systemIndex)
				result = "Drain Close Drain Auto"

			Case STATE_DrainOpenDrainManual2(systemIndex)
				result = "Drain Open Drain Manual 2"

			Case STATE_DrainCloseDrainManual2(systemIndex)
				result = "Drain Close Drain Manual 2"

			Case STATE_DrainCloseDrainAuto2(systemIndex)
				result = "Drain Close Drain Auto 2"

			Case STATE_DrainOpenDrainStart2(systemIndex)
				result = "Drain Open Drain Start 2"

			Case STATE_DrainOpenDrainCheck2(systemIndex)
				result = "Drain Open Drain Check 2"

			Case STATE_DrainCloseDrain2(systemIndex)
				result = "Drain Close Drain 2"

			Case STATE_DrainCloseTankVent(systemIndex)
				result = "Drain Close Tank Vent"

			Case STATE_DrainCloseSystemVentValveManualPOff(systemIndex)
				result = "Drain Close System Vent Valve Manual P Off"

				'
				'

			Case STATE_VacuumCloseDrain(systemIndex)
				result = "Vacuum Close Drain"

			Case STATE_VacuumOpenTransferValve(systemIndex)
				result = "Vacuum Open Transfer Valve"

			Case STATE_VacuumOpenHoseValve(systemIndex)
				result = "Vacuum Open Hose Valve"

			Case STATE_VacuumStartTransferSwitch(systemIndex)
				result = "Vacuum Start Transfer Switch"

			Case STATE_VacuumTransferSwitchOn(systemIndex)
				result = "Vacuum Transfer Switch On"

			Case STATE_VacuumIntoFilter(systemIndex)
				result = "Vacuum Into Filter"

			Case STATE_VacuumTransferComplete(systemIndex)
				result = "Vacuum Transfer Complete"

			Case STATE_FillVacuumHoseSelect(systemIndex)
				result = "Fill Vacuum Hose Select"

			Case STATE_DrainExitRepeat(systemIndex)
				result = "Drain Exit Repeat"

			Case STATE_DrainOpenDrain2(systemIndex)
				result = "Drain Open Drain 2"

			Case STATE_DrainRepeatLoopTop(systemIndex)
				result = "Drain Repeat Loop Top"

			Case STATE_DrainExitDrainLoop(systemIndex)
				result = "Drain Exit Drain Loop"

			Case STATE_DrainOpenTankVentManual2(systemIndex)
				result = "Drain Open Tank Vent Manual 2"

			Case STATE_DrainOpenSystemVentValveManual2(systemIndex)
				result = "Drain Open System Vent Valve Manual 2"

				'
				'

			Case STATE_PrecoatCloseHoseValve(systemIndex)
				result = "Precoat Close Hose Valve"

			Case STATE_PrecoatCloseTransferValve(systemIndex)
				result = "Precoat Close Transfer Valve"

			Case STATE_PrecoatOpenSystemVentValve(systemIndex)
				result = "Precoat Open System Vent Valve"

			Case STATE_PrecoatOpenTankVentPartial(systemIndex)
				result = "Precoat Open Tank Vent Partial"

			Case STATE_PrecoatOpenPumpThrottleValve(systemIndex)
				result = "Precoat Open Pump Throttle Valve"

			Case STATE_PrecoatStartPump(systemIndex)
				result = "Precoat Start Pump"

			Case STATE_PrecoatCloseTankVent(systemIndex)
				result = "Precoat Close Tank Vent"

			Case STATE_PrecoatCloseSystemVentValve(systemIndex)
				result = "Precoat Close System Vent Valve"

			Case STATE_PrecoatStopVacuumHose(systemIndex)
				result = "Precoat Stop Vacuum Hose"

			Case STATE_PrecoatOpenTankVent(systemIndex)
				result = "Precoat Open Tank Vent"

			Case STATE_PrecoatStopPump(systemIndex)
				result = "Precoat Stop Pump"

			Case STATE_PrecoatStartingPump(systemIndex)
				result = "Precoat Starting Pump"

			Case STATE_PrecoatOpenSystemVentValveManual(systemIndex)
				result = "Precoat Open System Vent Valve Manual"

			Case STATE_PrecoatCloseSystemVentValveManual(systemIndex)
				result = "Precoat Close System Vent Valve Manual"

			Case STATE_PrecoatCloseTankVentPOff(systemIndex)
				result = "Precoat Close Tank Vent P Off"

			Case STATE_PrecoatCloseSystemVentValveManualPOff(systemIndex)
				result = "Precoat Close System Vent Valve Manual P Off"

				'
				'

			Case STATE_BumpSetStart(systemIndex)
				result = "Bump Set Start"

			Case STATE_BumpSetBumpOff(systemIndex)
				result = "Bump Set Bump Off"

			Case STATE_BumpSetBumpOn(systemIndex)
				result = "Bump Set Bump On"

			Case STATE_BumpSetLoop(systemIndex)
				result = "Bump Set Loop"

			Case STATE_BumpSelect(systemIndex)
				result = "Bump Select"

			Case STATE_BumpAutoBumpOff(systemIndex)
				result = "Bump Auto Bump Off"

			Case STATE_BumpAutoBumpOn(systemIndex)
				result = "Bump Auto Bump On"

			Case STATE_FlexCleanLoop(systemIndex)
				result = "Flex Clean Loop"

			Case STATE_FlexCleanStartup(systemIndex)
				result = "Flex Clean Startup"

			Case STATE_FlexStartPump(systemIndex)
				result = "Flex Start Pump"

			Case STATE_FlexStopPump(systemIndex)
				result = "Flex Stop Pump"

			Case STATE_FlexWait(systemIndex)
				result = "Flex Wait"

			Case STATE_FlexOpenRegen(systemIndex)
				result = "Flex Open Regen"

			Case STATE_FlexPrecoat(systemIndex)
				result = "Flex Precoat"

			Case STATE_FlexCloseRegen(systemIndex)
				result = "Flex Close Regen"

			Case STATE_BumpStartup(systemIndex)
				result = "Bump Startup"

			Case STATE_FlexOpenTankVentManual(systemIndex)
				result = "Flex Open Tank Vent Manual"

			Case STATE_FlexOpenVacuumHoseValve(systemIndex)
				result = "Flex Open Vacuum Hose Valve"

			Case STATE_FlexLoadCleaner(systemIndex)
				result = "Flex Load Cleaner"

			Case STATE_FlexCloseTankVent(systemIndex)
				result = "Flex Close Tank Vent"

			Case STATE_FlexCloseVacuumHoseValve(systemIndex)
				result = "Flex Close Vacuum Hose Valve"

			Case STATE_FlexOpenPumpThrottleValve(systemIndex)
				result = "Flex Open Pump Throttle Valve"

			Case STATE_FlexBumpSetStart(systemIndex)
				result = "Flex Bump Set Start"

			Case STATE_FlexBumpAutoBumpOff(systemIndex)
				result = "Flex Bump Auto Bump Off"

			Case STATE_FlexBumpAutoBumpOn(systemIndex)
				result = "Flex Bump Auto Bump On"

			Case STATE_FlexBumpSetLoop(systemIndex)
				result = "Flex Bump Set Loop"

			Case STATE_FlexCloseDrainManual(systemIndex)
				result = "Flex Close Drain manual"

			Case STATE_FlexCleanLoopTop(systemIndex)
				result = "Flex Clean Loop Top"

			Case STATE_FlexDrainStartBump(systemIndex)
				result = "Flex Drain Start Bump"

			Case STATE_FlexDrainAutoBumpStart(systemIndex)
				result = "Flex Drain Auto Bump Start"

			Case STATE_FlexDrainAutoBumpOff(systemIndex)
				result = "Flex Drain Auto Bump Off"

			Case STATE_FlexDrainAutoBumpOn(systemIndex)
				result = "Flex Drain Auto Bump On"

			Case STATE_FlexDrainAutoBumpLoop(systemIndex)
				result = "Flex Drain Auto Bump Loop"

			Case STATE_FlexDrainBumpSelectB(systemIndex)
				result = "Flex Drain Bump Select B"

			Case STATE_FlexDrainClosePumpThrottleValve(systemIndex)
				result = "Flex Drain Close Pump Throttle Valve"

			Case STATE_FlexDrainOpenDrainManual(systemIndex)
				result = "Flex Drain Open Drain Manual"

			Case STATE_FlexDrainOpenTankVentManual(systemIndex)
				result = "Flex Drain Open Tank Vent Manual"

			Case STATE_FlexOpenSystemVentValveManual(systemIndex)
				result = "Flex Open System Vent Valve Manual"

			Case STATE_FlexDrainCloseDrainManual(systemIndex)
				result = "Flex Drain Close Drain Manual"

			Case STATE_FlexDrainCloseRegen(systemIndex)
				result = "Flex Drain Close Regen"

			Case STATE_FlexStopPump2(systemIndex)
				result = "Flex Stop Pump 2"



			Case STATE_BumpAutoFiremanOn(systemIndex)
				result = "Bump Auto Fireman On"

			Case STATE_BumpAutoOpenRegen(systemIndex)
				result = "Bump Auto Open Regen"

			Case STATE_BumpAutoCloseEffluent(systemIndex)
				result = "Bump Auto Close Effluent"
				'
				'

			Case STATE_DrainOpenTankVentManual(systemIndex)
				result = "Drain Open Tank Vent Manual"

			Case STATE_FillCloseDrain(systemIndex)
				result = "Fill Close Drain"

			Case STATE_FillCloseTankVent(systemIndex)
				result = "Fill Close Tank Vent"

			Case STATE_FillOpenTransferValve(systemIndex)
				result = "Fill Open Transfer Valve"

			Case STATE_FillOpenV3Valve(systemIndex)
				result = "Fill Open V3 Valve"

			Case STATE_FillStartVacuumHoseWait(systemIndex)
				result = "Fill Start Vacuum Hose Wait"

			Case STATE_FillStartVacuumHose(systemIndex)
				result = "Fill Start Vacuum Hose"

			Case STATE_FillStopVacuumHose(systemIndex)
				result = "Fill Stop Vacuum Hose"

			Case STATE_DrainRegenStart(systemIndex)
				result = "Drain Regen Start"

			Case STATE_DrainPumpOn(systemIndex)
				result = "Drain Pump On"

			Case STATE_DrainOpenRegen(systemIndex)
				result = "Drain Open Regen"

			Case STATE_DrainPrecoat(systemIndex)
				result = "Drain Precoat"

			Case STATE_DrainFiremanOn(systemIndex)
				result = "Drain Fireman On"

			Case STATE_DrainOpenEffluent(systemIndex)
				result = "Drain Open Effluent"

			Case STATE_DrainCloseRegen(systemIndex)
				result = "Drain Close Regen"

			Case STATE_FillOpenVacuumHoseValve(systemIndex)
				result = "Fill Open Vacuum Hose Valve"

			Case STATE_FillCloseVacuumHoseValve(systemIndex)
				result = "Fill Close Vacuum Hose Valve"

			Case STATE_FillCloseSystemVentValveManualPOff(systemIndex)
				result = "Fill Close System Vent Valve Manual P Off"

				'
				'

			Case STATE_IdleDrainProcessStop(systemIndex)
				result = "Idle Drain Process Stop"

			Case STATE_IdleCloseDrain(systemIndex)
				result = "Idle Close Drain"

			Case STATE_IdleCloseTankVent(systemIndex)
				result = "Idle Close Tank Valve"

			Case STATE_IdleStopVacuumHose(systemIndex)
				result = "Idle Stop Vacuum Hose"

			Case STATE_IdleCloseTransferValve(systemIndex)
				result = "Idle Close Transfer Valve"

			Case STATE_IdleCloseSystemVentValve(systemIndex)
				result = "Idle Close System Vent Valve"

			Case STATE_DrainToIdleStartup(systemIndex)
				result = "Drain To Idle Startup"

			Case STATE_IdleOpenPumpThrottleValve(systemIndex)
				result = "Idle Open Pump Throttle Valve"

			Case STATE_IdleCloseDrainManual(systemIndex)
				result = "Idle Close Drain Manual"

			Case STATE_IdleCloseSystemVentValveManual(systemIndex)
				result = "Idle Close System Vent Valve Manual"

			Case STATE_DrainOpenTankVent(systemIndex)
				result = "Drain Open Tank Vent"

			Case STATE_CouldNotReconnect(systemIndex)
				result = "Could Not Reconnect"
			Case STATE_Reconnecting(systemIndex)
				result = "Reconnecting"
			Case STATE_Reseting(systemIndex)
				result = "Reseting"

			Case Else
				result = "Unknown (0x" & State.ToString("X") & ")"
		End Select

		Return result
	End Function

	' given a state string, return a corresponding value
	Private Function GetStateIntegerFromStateString(ByRef state As String) As UShort
		Dim result As UShort = 0

		If state = "None" Then
			result = STATE_None(systemIndex)

		ElseIf state = "Set_Next_Step" Then
			result = STATE_SetNextStep(systemIndex)

		ElseIf state = "Error_No_Vacuum" Then
			result = STATE_ErrorNoVacuum(systemIndex)

		ElseIf state = "Error_Delta_P_Sensor" Then
			result = STATE_ErrorDeltaPSensor(systemIndex)

		ElseIf state = "Set_Previous_Step" Then
			result = STATE_SetPreviousStep(systemIndex)

		ElseIf state = "Error_No_Main_Pump" Then
			result = STATE_ErrorNoMainPump(systemIndex)

		ElseIf state = "Error_Pressure_Sensor" Then
			result = STATE_ErrorPressureSensor(systemIndex)

		ElseIf state = "Error_Bad_Upgrade" Then
			result = STATE_BadUpgradeError(systemIndex)

		ElseIf state = "Error_Flow_Sensor" Then
			result = STATE_ErrorFlowSensor(systemIndex)

		ElseIf state = "Aux_Check_State" Then
			result = STATE_AuxCheckState(systemIndex)

		ElseIf state = "Error_Valve_Over_Limit" Then
			result = STATE_ErrorValveOverLimit(systemIndex)

		ElseIf state = "Delta-p_Not_Resolved"  
			result = STATE_ErrorBumpFail(systemIndex)



		ElseIf state = "Filter" Then
			result = STATE_Filter(systemIndex)

		ElseIf state = "Filter_Startup" Then
			result = STATE_FilterStartup(systemIndex)

		ElseIf state = "Filter_Regen_Start" Then
			result = STATE_FilterRegenStart(systemIndex)

		ElseIf state = "Filter_Pump_On" Then
			result = STATE_FilterPumpOn(systemIndex)

		ElseIf state = "Filter_Open_Regen" Then
			result = STATE_FilterOpenRegen(systemIndex)

		ElseIf state = "Filter_Precoat" Then
			result = STATE_FilterPrecoat(systemIndex)

		ElseIf state = "Filter_Open_Effluent" Then
			result = STATE_FilterOpenEffluent(systemIndex)

		ElseIf state = "Filter_Close_Regen" Then
			result = STATE_FilterCloseRegen(systemIndex)

		ElseIf state = "Filter_Fireman_On" Then
			result = STATE_FilterFiremanOn(systemIndex)

		ElseIf state = "Filter_Regen_Open" Then
			result = STATE_FilterRegenOpen(systemIndex)

		ElseIf state = "Filter_Close_Effluent" Then
			result = STATE_FilterCloseEffluent(systemIndex)

		ElseIf state = "Filter_Drain_Open" Then
			result = STATE_FilterDrainOpen(systemIndex)

		ElseIf state = "Filter_Drain_Close" Then
			result = STATE_FilterDrainClose(systemIndex)

		ElseIf state = "Filter_Main_Pump_On" Then
			result = STATE_FilterMainPumpOn(systemIndex)

		ElseIf state = "Filter_Main_Pump_Off" Then
			result = STATE_FilterMainPumpOff(systemIndex)

		ElseIf state = "Filter_Precoat_Pump_Off" Then
			result = STATE_FilterPrecoatPumpOff(systemIndex)



		ElseIf state = "Idle_Startup" Then
			result = STATE_IdleStartup(systemIndex)

		ElseIf state = "Idle_Filter_Startup" Then
			result = STATE_IdleFilterStartup(systemIndex)

		ElseIf state = "Idle_Stop_Pump" Then
			result = STATE_IdleStopPump(systemIndex)

		ElseIf state = "Idle_Raise_Bump" Then
			result = STATE_IdleRaiseBump(systemIndex)

		ElseIf state = "Idle_Close_Regen" Then
			result = STATE_IdleCloseRegen(systemIndex)

		ElseIf state = "Idle_Close_Effluent" Then
			result = STATE_IdleCloseEffluent(systemIndex)

		ElseIf state = "Idle" Then
			result = STATE_Idle(systemIndex)

		ElseIf state = "Idle_Stop_Fireman" Then
			result = STATE_IdleStopFireman(systemIndex)

		ElseIf state = "Idle_Open_Regen" Then
			result = STATE_IdleOpenRegen(systemIndex)

		ElseIf state = "Maintenance_Mode" Then
			result = STATE_MaintenanceMode(systemIndex)



		ElseIf state = "Clean_Close_Strainer" Then
			result = STATE_CleanCloseStrainer(systemIndex)

		ElseIf state = "Clean_Basket" Then
			result = STATE_CleanBasket(systemIndex)

		ElseIf state = "Clean_Open_Strainer" Then
			result = STATE_CleanOpenStrainer(systemIndex)

		ElseIf state = "Clean_Start_Bump_Wait" Then
			result = STATE_CleanStartBumpWait(systemIndex)

		ElseIf state = "Clean_Start_Pump" Then
			result = STATE_CleanStartPump(systemIndex)

		ElseIf state = "Clean_Stop_Pump" Then
			result = STATE_CleanStopPump(systemIndex)

		ElseIf state = "Clean_Bump_Off" Then
			result = STATE_CleanBumpOff(systemIndex)

		ElseIf state = "Clean_Bump_On" Then
			result = STATE_CleanBumpOn(systemIndex)

		ElseIf state = "Main_Drain_On" Then
			result = STATE_MainDrainOn(systemIndex)

		ElseIf state = "Main_Drain_Off" Then
			result = STATE_MainDrainOff(systemIndex)

		ElseIf state = "PLVV_On" Then
			result = STATE_PLVVOn(systemIndex)

		ElseIf state = "PLVV_Off" Then
			result = STATE_PLVVOff(systemIndex)

		ElseIf state = "Strainer_Flip" Then
			result = STATE_StrainerFlip(systemIndex)



		ElseIf state = "Drain_Close_Pump_Throttle_Valve" Then
			result = STATE_DrainClosePumpThrottleValve(systemIndex)

		ElseIf state = "Drain_Start_Bump" Then
			result = STATE_DrainStartBump(systemIndex)

		ElseIf state = "Drain_Bump_Off" Then
			result = STATE_DrainBumpOff(systemIndex)

		ElseIf state = "Drain_Bump_On" Then
			result = STATE_DrainBumpOn(systemIndex)

		ElseIf state = "Drain_Open_Drain" Then
			result = STATE_DrainOpenDrain(systemIndex)

		ElseIf state = "Drain_Close_Drain" Then
			result = STATE_DrainCloseDrain(systemIndex)

		ElseIf state = "Drain_Start_Transfer_Switch" Then
			result = STATE_DrainStartTransferSwitch(systemIndex)

		ElseIf state = "Drain_Transfer_Switch_On" Then
			result = STATE_DrainTransferSwitchOn(systemIndex)

		ElseIf state = "Drain_Transfer_Switch_Off" Then
			result = STATE_DrainTransferSwitchOff(systemIndex)

		ElseIf state = "Drain_Open_Pump_Throttle_Valve" Then
			result = STATE_DrainOpenPumpThrottleValve(systemIndex)

		ElseIf state = "Drain_Start_Pump" Then
			result = STATE_DrainStartPump(systemIndex)

		ElseIf state = "Drain_Stop_Pump" Then
			result = STATE_DrainStopPump(systemIndex)

		ElseIf state = "Drain_Bump_Select" Then
			result = STATE_DrainBumpSelect(systemIndex)

		ElseIf state = "Drain_Starting_Pump" Then
			result = STATE_DrainStartingPump(systemIndex)

		ElseIf state = "Drain_Auto_Bump_Off" Then
			result = STATE_DrainAutoBumpOff(systemIndex)

		ElseIf state = "Drain_Auto_Bump_On" Then
			result = STATE_DrainAutoBumpOn(systemIndex)

		ElseIf state = "Drain_Auto_Bump_Loop" Then
			result = STATE_DrainAutoBumpLoop(systemIndex)

		ElseIf state = "Drain_Bump_Select_B" Then
			result = STATE_DrainBumpSelectB(systemIndex)

		ElseIf state = "Rinse_Repeat" Then
			result = STATE_RinseRepeat(systemIndex)

		ElseIf state = "Drain_To_Filter" Then
			result = STATE_DrainToFilter(systemIndex)

		ElseIf state = "Drain_Auto_Bump_Start" Then
			result = STATE_DrainAutoBumpStart(systemIndex)

		ElseIf state = "Drain_Open_Drain_Manual" Then
			result = STATE_DrainOpenDrainManual(systemIndex)

		ElseIf state = "Drain_Close_Drain_Manual" Then
			result = STATE_DrainCloseDrainManual(systemIndex)

		ElseIf state = "Drain_Open_Drain_Start" Then
			result = STATE_DrainOpenDrainStart(systemIndex)

		ElseIf state = "Drain_Open_Drain_Check" Then
			result = STATE_DrainOpenDrainCheck(systemIndex)

		ElseIf state = "Drain_Close_Drain_Auto" Then
			result = STATE_DrainCloseDrainAuto(systemIndex)

		ElseIf state = "Drain_Open_Drain_Manual_2" Then
			result = STATE_DrainOpenDrainManual2(systemIndex)

		ElseIf state = "Drain_Close_Drain_Manual_2" Then
			result = STATE_DrainCloseDrainManual2(systemIndex)

		ElseIf state = "Drain_Close_Drain_Auto_2" Then
			result = STATE_DrainCloseDrainAuto2(systemIndex)

		ElseIf state = "Drain_Open_Drain_Start_2" Then
			result = STATE_DrainOpenDrainStart2(systemIndex)

		ElseIf state = "Drain_Open_Drain_Check_2" Then
			result = STATE_DrainOpenDrainCheck2(systemIndex)

		ElseIf state = "Drain_Close_Drain_2" Then
			result = STATE_DrainCloseDrain2(systemIndex)

		ElseIf state = "Drain_Close_Tank_Vent" Then
			result = STATE_DrainCloseTankVent(systemIndex)

		ElseIf state = "Drain_Close_System_Vent_Valve_Manual_P_Off" Then
			result = STATE_DrainCloseSystemVentValveManualPOff(systemIndex)



		ElseIf state = "Vacuum_Close_Drain" Then
			result = STATE_VacuumCloseDrain(systemIndex)

		ElseIf state = "Vacuum_Open_Transfer_Valve" Then
			result = STATE_VacuumOpenTransferValve(systemIndex)

		ElseIf state = "Vacuum_Open_Hose_Valve" Then
			result = STATE_VacuumOpenHoseValve(systemIndex)

		ElseIf state = "Vacuum_Start_Transfer_Switch" Then
			result = STATE_VacuumStartTransferSwitch(systemIndex)

		ElseIf state = "Vacuum_Transfer_Switch_On" Then
			result = STATE_VacuumTransferSwitchOn(systemIndex)

		ElseIf state = "Vacuum_Into_Filter" Then
			result = STATE_VacuumIntoFilter(systemIndex)

		ElseIf state = "Vacuum_Transfer_Complete" Then
			result = STATE_VacuumTransferComplete(systemIndex)

		ElseIf state = "Fill_Vacuum_Hose_Select" Then
			result = STATE_FillVacuumHoseSelect(systemIndex)

		ElseIf state = "Drain_Exit_Repeat" Then
			result = STATE_DrainExitRepeat(systemIndex)

		ElseIf state = "Drain_Open_Drain_2" Then
			result = STATE_DrainOpenDrain2(systemIndex)

		ElseIf state = "Drain_Repeat_Loop_Top" Then
			result = STATE_DrainRepeatLoopTop(systemIndex)

		ElseIf state = "Drain_Exit_Drain_Loop" Then
			result = STATE_DrainExitDrainLoop(systemIndex)

		ElseIf state = "Drain_Open_Tank_Vent_Manual_2" Then
			result = STATE_DrainOpenTankVentManual2(systemIndex)

		ElseIf state = "Drain_Open_System_Vent_Valve_Manual_2" Then
			result = STATE_DrainOpenSystemVentValveManual2(systemIndex)



		ElseIf state = "Precoat_Close_Hose_Valve" Then
			result = STATE_PrecoatCloseHoseValve(systemIndex)

		ElseIf state = "Precoat_Close_Transfer_Valve" Then
			result = STATE_PrecoatCloseTransferValve(systemIndex)

		ElseIf state = "Precoat_Open_System_Vent_Valve" Then
			result = STATE_PrecoatOpenSystemVentValve(systemIndex)

		ElseIf state = "Precoat_Open_Tank_Vent_Partial" Then
			result = STATE_PrecoatOpenTankVentPartial(systemIndex)

		ElseIf state = "Precoat_Open_Pump_Throttle_Valve" Then
			result = STATE_PrecoatOpenPumpThrottleValve(systemIndex)

		ElseIf state = "Precoat_Start_Pump" Then
			result = STATE_PrecoatStartPump(systemIndex)

		ElseIf state = "Precoat_Close_Tank_Vent" Then
			result = STATE_PrecoatCloseTankVent(systemIndex)

		ElseIf state = "Precoat_Close_System_Vent_Valve" Then
			result = STATE_PrecoatCloseSystemVentValve(systemIndex)

		ElseIf state = "Precoat_Stop_Vacuum_Hose" Then
			result = STATE_PrecoatStopVacuumHose(systemIndex)

		ElseIf state = "Precoat_Open_Tank_Vent" Then
			result = STATE_PrecoatOpenTankVent(systemIndex)

		ElseIf state = "Precoat_Stop_Pump" Then
			result = STATE_PrecoatStopPump(systemIndex)

		ElseIf state = "Precoat_Starting_Pump" Then
			result = STATE_PrecoatStartingPump(systemIndex)

		ElseIf state = "Precoat_Open_System_Vent_Valve_Manual" Then
			result = STATE_PrecoatOpenSystemVentValveManual(systemIndex)

		ElseIf state = "Precoat_Close_System_Vent_Valve_Manual" Then
			result = STATE_PrecoatCloseSystemVentValveManual(systemIndex)

		ElseIf state = "Precoat_Close_Tank_Vent_P_Off" Then
			result = STATE_PrecoatCloseTankVentPOff(systemIndex)

		ElseIf state = "Precoat_Close_System_Vent_Valve_Manual_P_Off" Then
			result = STATE_PrecoatCloseSystemVentValveManualPOff(systemIndex)



		ElseIf state = "Bump_Set_Start" Then
			result = STATE_BumpSetStart(systemIndex)

		ElseIf state = "Bump_Set_Bump_Off" Then
			result = STATE_BumpSetBumpOff(systemIndex)

		ElseIf state = "Bump_Set_Bump_On" Then
			result = STATE_BumpSetBumpOn(systemIndex)

		ElseIf state = "Bump_Set_Loop" Then
			result = STATE_BumpSetLoop(systemIndex)

		ElseIf state = "Bump_Select" Then
			result = STATE_BumpSelect(systemIndex)

		ElseIf state = "Bump_Auto_Bump_Off" Then
			result = STATE_BumpAutoBumpOff(systemIndex)

		ElseIf state = "Bump_Auto_Bump_On" Then
			result = STATE_BumpAutoBumpOn(systemIndex)

		ElseIf state = "Flex_Clean_Loop" Then
			result = STATE_FlexCleanLoop(systemIndex)

		ElseIf state = "Flex_Clean_Startup" Then
			result = STATE_FlexCleanStartup(systemIndex)

		ElseIf state = "Flex_Start_Pump" Then
			result = STATE_FlexStartPump(systemIndex)

		ElseIf state = "Flex_Stop_Pump" Then
			result = STATE_FlexStopPump(systemIndex)

		ElseIf state = "Flex_Wait" Then
			result = STATE_FlexWait(systemIndex)

		ElseIf state = "Flex_Open_Regen" Then
			result = STATE_FlexOpenRegen(systemIndex)

		ElseIf state = "Flex_Precoat" Then
			result = STATE_FlexPrecoat(systemIndex)

		ElseIf state = "Flex_Close_Regen" Then
			result = STATE_FlexCloseRegen(systemIndex)

		ElseIf state = "Bump_Startup" Then
			result = STATE_BumpStartup(systemIndex)

		ElseIf state = "Flex_Open_Tank_Vent_Manual" Then
			result = STATE_FlexOpenTankVentManual(systemIndex)

		ElseIf state = "Flex_Open_Vacuum_Hose_Valve" Then
			result = STATE_FlexOpenVacuumHoseValve(systemIndex)

		ElseIf state = "Flex_Load_Cleaner" Then
			result = STATE_FlexLoadCleaner(systemIndex)

		ElseIf state = "Flex_Close_Tank_Vent" Then
			result = STATE_FlexCloseTankVent(systemIndex)

		ElseIf state = "Flex_Close_Vacuum_Hose_Valve" Then
			result = STATE_FlexCloseVacuumHoseValve(systemIndex)

		ElseIf state = "Flex_Open_Pump_Throttle_Valve" Then
			result = STATE_FlexOpenPumpThrottleValve(systemIndex)

		ElseIf state = "Flex_Bump_Set_Start" Then
			result = STATE_FlexBumpSetStart(systemIndex)

		ElseIf state = "Flex_Bump_Auto_Bump_Off" Then
			result = STATE_FlexBumpAutoBumpOff(systemIndex)

		ElseIf state = "Flex_Bump_Auto_Bump_On" Then
			result = STATE_FlexBumpAutoBumpOn(systemIndex)

		ElseIf state = "Flex_Bump_Set_Loop" Then
			result = STATE_FlexBumpSetLoop(systemIndex)

		ElseIf state = "Flex_Close_Drain_manual" Then
			result = STATE_FlexCloseDrainManual(systemIndex)

		ElseIf state = "Flex_Clean_Loop_Top" Then
			result = STATE_FlexCleanLoopTop(systemIndex)

		ElseIf state = "Flex_Drain_Start_Bump" Then
			result = STATE_FlexDrainStartBump(systemIndex)

		ElseIf state = "Flex_Drain_Auto_Bump_Start" Then
			result = STATE_FlexDrainAutoBumpStart(systemIndex)

		ElseIf state = "Flex_Drain_Auto_Bump_Off" Then
			result = STATE_FlexDrainAutoBumpOff(systemIndex)

		ElseIf state = "Flex_Drain_Auto_Bump_On" Then
			result = STATE_FlexDrainAutoBumpOn(systemIndex)

		ElseIf state = "Flex_Drain_Auto_Bump_Loop" Then
			result = STATE_FlexDrainAutoBumpLoop(systemIndex)

		ElseIf state = "Flex_Drain_Bump_Select_B" Then
			result = STATE_FlexDrainBumpSelectB(systemIndex)

		ElseIf state = "Flex_Drain_Close_Pump_Throttle_Valve" Then
			result = STATE_FlexDrainClosePumpThrottleValve(systemIndex)

		ElseIf state = "Flex_Drain_Open_Drain_Manual" Then
			result = STATE_FlexDrainOpenDrainManual(systemIndex)

		ElseIf state = "Flex_Drain_Open_Tank_Vent_Manual" Then
			result = STATE_FlexDrainOpenTankVentManual(systemIndex)

		ElseIf state = "Flex_Open_System_Vent_Valve_Manual" Then
			result = STATE_FlexOpenSystemVentValveManual(systemIndex)

		ElseIf state = "Flex_Drain_Close_Drain_Manual" Then
			result = STATE_FlexDrainCloseDrainManual(systemIndex)

		ElseIf state = "Flex_Drain_Close_Regen" Then
			result = STATE_FlexDrainCloseRegen(systemIndex)

		ElseIf state = "Flex_Stop_Pump_2" Then
			result = STATE_FlexStopPump2(systemIndex)



		ElseIf state = "Bump_Auto_Fireman_On" then
			result = STATE_BumpAutoFiremanOn(systemIndex)

		ElseIf state = "Bump_Auto_Open_Regen" then
			result = STATE_BumpAutoOpenRegen(systemIndex)

		ElseIf state = "Bump_Auto_Close_Effluent" then
			result = STATE_BumpAutoCloseEffluent(systemIndex)



		ElseIf state = "Drain_Open_Tank_Vent_Manual" Then
			result = STATE_DrainOpenTankVentManual(systemIndex)

		ElseIf state = "Fill_Close_Drain" Then
			result = STATE_FillCloseDrain(systemIndex)

		ElseIf state = "Fill_Close_Tank_Vent" Then
			result = STATE_FillCloseTankVent(systemIndex)

		ElseIf state = "Fill_Open_Transfer_Valve" Then
			result = STATE_FillOpenTransferValve(systemIndex)

		ElseIf state = "Fill_Open_V3_Valve" Then
			result = STATE_FillOpenV3Valve(systemIndex)

		ElseIf state = "Fill_Start_Vacuum_Hose_Wait" Then
			result = STATE_FillStartVacuumHoseWait(systemIndex)

		ElseIf state = "Fill_Start_Vacuum_Hose" Then
			result = STATE_FillStartVacuumHose(systemIndex)

		ElseIf state = "Fill_Stop_Vacuum_Hose" Then
			result = STATE_FillStopVacuumHose(systemIndex)

		ElseIf state = "Drain_Regen_Start" Then
			result = STATE_DrainRegenStart(systemIndex)

		ElseIf state = "Drain_Pump_On" Then
			result = STATE_DrainPumpOn(systemIndex)

		ElseIf state = "Drain_Open_Regen" Then
			result = STATE_DrainOpenRegen(systemIndex)

		ElseIf state = "Drain_Precoat" Then
			result = STATE_DrainPrecoat(systemIndex)

		ElseIf state = "Drain_Fireman_On" Then
			result = STATE_DrainFiremanOn(systemIndex)

		ElseIf state = "Drain_Open_Effluent" Then
			result = STATE_DrainOpenEffluent(systemIndex)

		ElseIf state = "Drain_Close_Regen" Then
			result = STATE_DrainCloseRegen(systemIndex)

		ElseIf state = "Fill_Open_Vacuum_Hose_Valve" Then
			result = STATE_FillOpenVacuumHoseValve(systemIndex)

		ElseIf state = "Fill_Close_Vacuum_Hose_Valve" Then
			result = STATE_FillCloseVacuumHoseValve(systemIndex)

		ElseIf state = "Fill_Close_System_Vent_Valve_Manual_P_Off" Then
			result = STATE_FillCloseSystemVentValveManualPOff(systemIndex)




		ElseIf state = "Idle_Drain_Process_Stop" Then
			result = STATE_IdleDrainProcessStop(systemIndex)

		ElseIf state = "Idle_Close_Drain" Then
			result = STATE_IdleCloseDrain(systemIndex)

		ElseIf state = "Idle_Close_Tank_Valve" Then
			result = STATE_IdleCloseTankVent(systemIndex)

		ElseIf state = "Idle_Stop_Vacuum_Hose" Then
			result = STATE_IdleStopVacuumHose(systemIndex)

		ElseIf state = "Idle_Close_Transfer_Valve" Then
			result = STATE_IdleCloseTransferValve(systemIndex)

		ElseIf state = "Idle_Close_System_Vent_Valve" Then
			result = STATE_IdleCloseSystemVentValve(systemIndex)

		ElseIf state = "Drain_To_Idle_Startup" Then
			result = STATE_DrainToIdleStartup(systemIndex)

		ElseIf state = "Idle_Open_Pump_Throttle_Valve" Then
			result = STATE_IdleOpenPumpThrottleValve(systemIndex)

		ElseIf state = "Idle_Close_Drain_Manual" Then
			result = STATE_IdleCloseDrainManual(systemIndex)

		ElseIf state = "Idle_Close_System_Vent_Valve_Manual" Then
			result = STATE_IdleCloseSystemVentValveManual(systemIndex)

		ElseIf state = "Drain_Open_Tank_Vent" Then
			result = STATE_DrainOpenTankVent(systemIndex)



		ElseIf state = "Could_Not_Reconnect"
			result = STATE_CouldNotReconnect(systemIndex)

		ElseIf state = "Reconnecting"
			result = STATE_Reconnecting(systemIndex)

		ElseIf state = "Reseting"
			result = STATE_Reseting(systemIndex)

		Else
			' leave the value as it is.
			result = STATE_None(systemIndex)
		End If

		Return result
	End Function
#End Region

	Sub ProcessThread()
		Dim thread         As Thread  = Thread.CurrentThread
		Dim retval         As Integer = 0
		Dim curLine        As String  = ""
		Dim LineNo         As Integer = 0
		Dim sep()          As Char    = {" ", ":"}
		Dim isDebugBox     As UShort  = False
		Dim today          As String  = Date.Now.Year & "-" & Date.Now.Month.ToString("D2") & "-" & Date.Now.Day.ToString("D2")
		Dim canExecuteLine As Boolean = True
		Dim WebResponse    As String  = ""
		Dim obj                       = Nothing
		Dim commandFile    As String  = ""
		Dim commandString  As String  = ""
		Dim result		   As String  = ""
		Dim errorMessage   As String  = ""

		Dim mbServerClass  As New ModbusXfce()
		Dim jsonapi        As New JSON_API()

		' prepare the modbus server
		mbServerClass.SetSwapFlag(ModbusXfce.SwapByteFlag Or ModbusXfce.SwapWordFlag)
		mbServerClass.EnableLogging(My.Settings.SaveResultsLocation & "\" & today & "_Runner\" & thisServerInfo.ServerName & "-Client.txt")
		mbServerClass.EnableMessageLogging()

		' open the connection
		If (0 = mbServerClass.OpenTCP(thisServerInfo.ServerName, 502)) Then
			errorMessage = "Could not connect: " & thisServerInfo.ServerName
			GoTo DONE_NF
		End If

		' get the model of the system
		If jsonapi.GetMachineInfo(thisServerInfo.ServerName, WebResponse) = False Then
			errorMessage = "----- Initial Get Machine Info -----" & vbNewLine & WebResponse
            GoTo DONE_NF
		End If

		' try to deserialize the JSON
		Try
            obj = JsonConvert.DeserializeObject(Of JSON_InfoResult)(WebResponse)
        Catch
            errorMessage = "----- Initial Get Machine Info -----" & vbNewLine & 
				           "Could not convert JSON result string" & vbNewLine & WebResponse
            GoTo DONE_NF
        End Try

		thisModel = obj.model.ToString()

		' figure out what index we are using based off the model
		Select Case thisModel
			Case MODEL_REBIG
				systemIndex = INDEX_REBIG

			Case MODEL_D7
				systemIndex = INDEX_D7

#If NotSupported
			Case MODEL_2300
				systemIndex = INDEX_2300
#End If
			Case Else
				errorMessage = "We are not set up for " & thisModel
				GoTo DONE_NF
		End Select

		If thisModel <> MODEL_2300 Then
			If jsonapi.SetParameters(thisServerInfo.ServerName, thisModel, obj.cpuid, WebResponse) = False  Then
				errorMessage = "Could not set the parameters throught JSON: " & thisServerInfo.ServerName & vbNewLine & 
					vbNewLine & 
					WebResponse
				GoTo DONE_NF
			End If
		End If

#If 1
		' check to see if we need to change to a static IP
		If thisIsFixedIP = true Then
			' configure the IP
			If jsonapi.ConfigureFixedIP(thisServerInfo.ServerName, thisModel, result) = False Then
				errorMessage = result
				GoTo DONE_NF
			End If

			logHandle.WriteLine(thisServerInfo.ServerName & " -> " & result)

			thisServerInfo.ServerName = result

			' give the box enough time to shut down before we try to reconnect
			mbServerClass.CloseHandle()

			Thread.Sleep(MILLISECONDS_IN_SECONDS * 10)

			Dim connected As Boolean = False

			' set the end time from now based off of what we passed in
			Dim waitTime As Date = Date.Now().AddHours(0).AddMinutes(10).AddSeconds(0)

			Do While Date.Now() < waitTime
				' first, delay for a second
				Thread.Sleep(MILLISECONDS_IN_SECONDS)

				' try to connect
				If (0 <> mbServerClass.OpenTCP(thisServerInfo.ServerName, 502)) Then
					connected = True
					Exit Do
				End If

				' check for the kill switch
				If thisServerInfo.ExitCommand = 1 Then
					GoTo DONE_NF
				End If
			Loop

			' check to see if we connected
			If connected = False Then
				errorMessage = "Could not connect after assigning new IP: " & thisServerInfo.ServerName

				GoTo DONE_NF
			End If
		End If
#End If

		thisServerInfo.IPStable = 1

		' set up our values for some of the variables

		Dim sqlapi As New SQL_API()
		sqlapi._Username = user
		sqlapi._Password = pass

		' try to open up the database associated
		If Bypass = False Then
			If sqlapi.SetDatabase(thisModel, WebResponse) = False Then
				errorMessage = "Could not Set up Database: " & WebResponse
				GoTo DONE_NF
			End If
		End If
		
		thisSerialNumber = obj.serial.ToString
		thisCPUVersion = obj.version.ToString.Substring(1)
		commandFile = My.Settings.CommandDirectory & "\" & thisModel & ".txt"

#If DebugScript = 1 
		commandFile = My.Settings.CommandDirectory & "\" & thisModel & "DebugScript.txt"
#End If

		' check to see if we need to do any database work
		If Bypass = False Then
			If sqlapi.FindSystemSerialNumber(obj.serial, result) = False Then
				Dim yesorno As Integer = MessageBox.Show("This system Serial number is not in the database. Would you like to continue?", "Continue?", MessageBoxButtons.YesNo)
				If yesorno <> DialogResult.Yes Then
					GoTo DONE_NF
				End If
			End If

			dim resultBoolean = sqlapi.BurnInDate(obj.serial, result)

			If resultBoolean = False Then
				MsgBox(result)
				GoTo DONE_NF
			End If
		End If

		' check to see if we have the file
		If File.Exists(commandFile) = false Then
			MsgBox("Command file '" & thisModel & ".txt' was not found in " & My.Settings.CommandDirectory & ".")
            GoTo DONE_NF
		End if

		' prepare log file
		If Directory.Exists(My.Settings.SaveResultsLocation & "\" & today & "_Runner") = False Then
			Directory.CreateDirectory(My.Settings.SaveResultsLocation & "\" & today & "_Runner")
		End If

		Dim LogFileName As String = My.Settings.SaveResultsLocation & "\" & today & "_Runner\" & thisServerInfo.ServerName & ".log"
		logHandle = New StreamWriter(LogFileName, True)
		logHandle.WriteLine(Date.Now.ToString("G") & "  Starting Test Run for " & thisModel)
		logHandle.Flush()

		DisplayIP()
		DisplayModel()

#If 0 Then
		' find out if we are in debug or not
		retval = mbServerClass.GetWord(MODBUS_IsInDebug(systemIndex), isDebugBox)
		If retval <> 0 Then
			logHandle.WriteLine(Date.Now().ToString("G") & " LINE: " & LineNo.ToString() & " This unit is not in Debug Mode.")
			logHandle.Flush()
		End If
#End If
		
		' load up the simulated digin values so we have them
		retval = mbServerClass.GetWord(MODBUS_SimulatedDigitalIn(systemIndex), diginValues)

		' open the command file
		Dim fileHandle As StreamReader = New StreamReader(commandFile)

		'
		' top of main process loop
		'
TOP:

#Region "CONTINUE LOGIC"
		' check to see if we got the exit command from the main process
		If thisServerInfo.ExitCommand = 1 Then
			GoTo DONE
		End If

		' read the next line from the command file
		curLine = fileHandle.ReadLine()

		' we should only get 'nothing' if we are at the end of the file
		If curLine Is Nothing Then
			GoTo DONE
		End If

		' increase our line number
		LineNo += 1

#If DebugSkip = 1
		If LineNo < Line Then
			GoTo TOP
		End If
#End IF

#If DebugLinePause = 1
		If LineNo = pauseLine Then
			Console.Out.WriteLine("At pauseLine.")
		End If
#End IF

		' if we have a black line
		If curLine.Length = 0 Then
			GoTo TOP
		End If

		' if we are a special character delimiter used for comments
		If GetChar(curLine, 1) = CChar("'") Or
		   GetChar(curLine, 1) = CChar("#") Then
			GoTo TOP
		End If

		' check to see if we are able to execute the current line
		' the only time that we are in this state is when we reslut a false from an 'IF' keyword
		' the only way to get out is when we pass our 'ENDIF' keyword
		' NO NESTING IS ALLOWED
		If canExecuteLine = False Then
			If curLine = "ENDIF" Then
				canExecuteLine = True
			End If

			GoTo TOP
		End If
#End Region

		' update our CurrentLine for the GUI
		curLine = Trim(curLine)
		CurrentLine = LineNo.ToString() & "-" & curLine

		' get the current state of the system
		retval = mbServerClass.GetWord(MODBUS_CurrentStateID(systemIndex), CurrentState)
		If retval <> 0 Then
			Dim message As String = "Could not get System state (rc=0x" & retval.ToString("X") & ")"
			LogError(mbServerClass, LineNo, message)
		End If

		' update the GUI
		UpdateGUI()
		If thisServerInfo.ExitCommand = 1 Then
			GoTo DONE
		End If

#If 0 then
		' repopulate the list of possible commands
		For index As Integer = 0 To maxCommands - 1
			CurrentValidCommands(index) = 0
		Next
		retval = mbServerClass.GetMultipleWords(MODBUS_CurrentCommands(systemIndex), CurrentValidCommands(0), maxCommands)
#End If

		' clear the old array of params and read in the new ones
		Dim Params As String() = Nothing
		Params = curLine.Split(sep, 30)

		commandString = Params(0)

		Select Case commandString
		    Case "ANALOGVALUE"
#Region "ANALOGVALUE"
				'-----------------------------------
				' A N A L O G V A L U E   C o m m a n d
				'-----------------------------------
				Dim AnalogNo As Integer = Params(1)
				Dim AnalogValue As Single = Params(2)

				' check to see if we have a valid analog number
				If 1 <= AnalogNo And AnalogNo <= maxAnalogChannels(systemIndex) Then

					' check to see if we have a valid analog value
					If 0 <= AnalogValue And AnalogValue <= 20.0 Then
						retval = mbServerClass.SetSingle(MODBUS_SimulatedAnalogIn(systemIndex) + (2 * (AnalogNo - 1)), AnalogValue)
						If retval <> 0 Then
							Dim message As String = "Could not set Analog " & Params(1) & " value: " & Params(1) & " (rc=0x" & retval.ToString("X") & ")"
							LogError(mbServerClass, LineNo, message)
						End If
					End If
				Else
					'Analog # is not valid, log what it is
					Dim message As String = "Analog # is not valid: " & Params(1)
					LogError(mbServerClass, LineNo, message)
				End If
#End Region
			Case "CHECKFILTERRELAY"
#Region "CHECKFILTERRELAY"
				'-----------------------------------
				' C H E C K F I L T E R R E L A Y   C o m m a n d 
				'-----------------------------------
				Dim relayNumber = Params(1)
				Dim expectedRelaySate = Params(2)
				Dim currentRelayState = 0

				' get the current relay bitmask
				retval = mbServerClass.GetWord(MODBUS_RelayMemory(systemIndex), currentRelayState)
				If retval <> 0 Then
					Dim message As String = "Could not get Filter Relay value: (rc=0x" & retval.ToString("X") & ")"
					LogError(mbServerClass, LineNo, message)
				Else
					' bit shift to get to our bit that we are comparing
					currentRelayState >>= (relayNumber - 1)

					' AND our answer with 1 so we will be left with 1 or 0
					currentRelayState = currentRelayState And 1

					' check to see if we are what we expect
					If currentRelayState <> expectedRelaySate Then
						Dim message As String = "Relay Current State [" & currentRelayState & "] does not match what is expected: [" & expectedRelaySate & "]"
						LogError(mbServerClass, LineNo, message)
					End If
				End If
#End Region
			Case "CHECKMODBUS"
#Region "CHECKMODBUS"
				'-----------------------------------
				' C H E C K M O D B U S   C o m m a n d
				'-----------------------------------
				Dim expectedValue As UShort = CUShort(Params(2))
				Dim currentValue As UShort

				retval = mbServerClass.GetWord(Params(1), currentValue)
				If retval <> 0 Then
					Dim message As String = "Could not get value for: " & Params(1) & " (rc=0x" & retval.ToString("X") & ")"
					LogError(mbServerClass, LineNo, message)
				Else
					' chcek to see if we are the same
					If expectedValue <> currentValue Then
						Dim message As String = "Word does not match expected from " & Params(1) & " (expected: " & expectedValue & " have: " & currentValue & ")"
						LogError(mbServerClass, LineNo, message)
					End If
				End If
#End Region
			Case "CHECKMODE"
#Region "CHECKMODE"
				'-----------------------------------
				' C H E C K M O D E   C o m m a n d
				'-----------------------------------

				' get the State Integer that we are wanting to check for.
				Dim MBValueW As UShort = GetStateIntegerFromStateString(Params(1))

				' get the current string of what our state is (for the error log if needed)
				Dim currentStateString As String = GetStateStringFromStateInteger(currentState)

				' check to see if we match
				If MBValueW <> currentState Then
					Dim message As String = "Current state not expected (expected: " & Params(1) & " have: " & currentStateString & ")"
					LogError(mbServerClass, LineNo, message)
				End If
#End Region
			Case "CHECKDWORD"
#Region "CHECKDWORD"
				'-----------------------------------
				' C H E C K D W O R D   C o m m a n d
				'-----------------------------------
				Dim MBAddress As Integer
				Dim tempDword As UInteger
				Dim checkDword As UInteger
				MBAddress = CInt(Params(1))
				tempDword = CUInt(Params(2))

				retval = mbServerClass.GetDword(MBAddress, checkDword)
				If retval <> 0 Then
					Dim message As String = "Could not get Double Word value: " & Params(1) & " (rc=0x" & retval.ToString("X") & ")"
					LogError(mbServerClass, LineNo, message)
				Else
					If tempDword <> checkDword Then
						Dim message As String = "Double Word does not match expected (expected: " & Params(2) & " have: " & checkDword.ToString() & ")"
						LogError(mbServerClass, LineNo, message)
					End If
				End If
#End Region
			Case "CHECKWORD"
#Region "CHECKWORD"
				'-----------------------------------
				' C H E C K W O R D   C o m m a n d
				'-----------------------------------
				Dim MBAddress As Integer
				Dim tempWord As UShort
				Dim checkWord As UShort
				MBAddress = CInt(Params(1))
				tempWord = CUShort(Params(2))

				retval = mbServerClass.GetWord(MBAddress, checkWord)
				If retval <> 0 Then
					Dim message As String = "Could not get Word value: " & Params(1) & " (rc=0x" & retval.ToString("X") & ")"
					LogError(mbServerClass, LineNo, message)
				Else
					If tempWord <> checkWord Then
						Dim message As String = "Word does not match expected (expected: " & Params(2) & " have: " & checkWord.ToString() & ")"
						LogError(mbServerClass, LineNo, message)
					End If
				End If			
#End Region
			Case "CLEARALARMS"
#Region "CLEARALARMS"
				'-----------------------------------
				' C L E A R A L A R M S    C o m m a n d
				'-----------------------------------
				retval = mbServerClass.SetWord(MODBUS_InputTaskCommand(systemIndex), TASK_ClearAllAlarms(systemIndex))
				If retval <> 0 Then
					Dim message As String = "Could not clear alarms (rc=0x" & retval.ToString("X") & ")"
					LogError(mbServerClass, LineNo, message)
				End If
#End Region
			Case "DELAY"
#Region "DELAY"
				'-----------------------------------
				' D E L A Y   C o m m a n d
				'-----------------------------------
				Dim hours As Integer = Params(1)
				Dim minutes As Integer = Params(2)
				Dim seconds As Integer = Params(3)
				Dim endTime As Date = Date.Now().AddHours(hours).AddMinutes(minutes).AddSeconds(seconds)

				Do While Date.Now() < endTime
					' check to see if we were given the exit command
					If thisServerInfo.ExitCommand = 1 Then
						GoTo DONE
					End If

					' delay for a second
					Thread.Sleep(MILLISECONDS_IN_SECONDS)

					' get the current state of the system and update
					retval = mbServerClass.GetWord(MODBUS_CurrentStateID(systemIndex), CurrentState)
					If retval <> 0 Then
						Dim message As String = "Could not get System state (rc=0x" & retval.ToString("X") & ")"
						LogError(mbServerClass, LineNo, message)
					Else
						DisplayCurrentState()
					End If
				Loop
#End Region
			Case "DIGINVALUE"
#Region "DIGINVALUE"
				'-----------------------------------
				' D I G I N V A L U E   C o m m a n d
				'-----------------------------------
				Dim DiginNo = CInt(Params(1))

				' check to see if we are working with a valid input
				If 1 <= DiginNo And DiginNo <= maxDigitalChannels(systemIndex) Then
					Dim DigValue As UShort = CUShort(Params(2))

					' if we have a valid input, do silly stuff to mask values on and off
					If DigValue = 0 Then
						diginValues = diginValues And OffMask(DiginNo - 1)
					Else
						diginValues = diginValues Or OnMask(DiginNo - 1)
					End If

					' try to set the new digital input bitmask
					retval = mbServerClass.SetWord(MODBUS_SimulatedDigitalIn(systemIndex), diginValues)
					If retval <> 0 Then
						Dim message As String = "Could not set Digital bit mask: " & Params(1) & " (rc=0x" & retval.ToString("X") & ")"
						LogError(mbServerClass, LineNo, message)
					End If
				Else
					Dim message As String = "Digital input number is not valid: " & Params(1)
					LogError(mbServerClass, LineNo, message)
				End If
#End Region
			Case "ENDIF"
#Region "ENDIF"
				'-----------------------------------
				' E N D I F   C o m m a n d
				'-----------------------------------
				' We do nothing here since it's purpose is done at the begining of the read line. All this does is account for it as a KEY WORD.
#End Region
			Case "EXIT"
#Region "EXIT"
				'-----------------------------------
				' E X I T   C o m m a n d
				'-----------------------------------
				GoTo DONE
#End Region
			Case "GOTO"
#Region "GOTO"
				'-----------------------------------
				' G O T O   C o m m a n d
				'-----------------------------------

				fileHandle.Close()
				fileHandle = New StreamReader(commandFile)
				Dim targetLabel = Params(1)
				Dim tempLineno As Integer = 0

				While (True)
					curLine = fileHandle.ReadLine()

					If curLine Is Nothing Then
						' we have reached the end of the file and did not find the label.
						logHandle.WriteLine(Date.Now().ToString("G") & "ERROR LINE: " & LineNo.ToString() & " Could not find GOTO Label " & Params(1))
						logHandle.Flush()
						GoTo DONE
					End If

					' keep track of line #
					tempLineno += 1

					' check to see if we have a line
					If curLine.Length > 0 Then

						' check to see that we have a ":" in the first character
						If GetChar(curLine, 1) = CChar(":") Then

							' split out the label from the colon
							Params = curLine.Split(sep, 8)

							' check to see if this is the label that we are looking for
							If targetLabel = Params(1) Then
								LineNo = tempLineno
								LoopCount += 1

								' if we are going back to the top and using the SQL and have a valid unit, we need to update the database
								If thisSerialNumber <> "no" And Bypass = False And targetLabel = "TOP" Then
									Dim results As String = ""

									Dim connected As Boolean = True

									' check to see if we are connected or not
									If sqlapi.conn.State <> ConnectionState.Open Then
										logHandle.WriteLine(Date.Now().ToString("G") & " SQL LINE: " & LineNo.ToString() & " Connection was found closed. Attempting to re-connect")
										logHandle.Flush()

										' try to connect to the database
										If sqlapi.OpenDatabase(results) = False Then
											logHandle.WriteLine(Date.Now().ToString("G") & " SQL LINE: " & LineNo.ToString() & " Was not able to re-connect: " & results)
											logHandle.Flush()
											connected = false
										Else
											logHandle.WriteLine(Date.Now().ToString("G") & " SQL LINE: " & LineNo.ToString() & " re-connect: SUCCESS ")
											logHandle.Flush()
										End If
									End If

									' if we are still connected then update the database
									If connected = True Then
										If sqlapi.BurnInAudit(thisSerialNumber, results, LoopCount, errorCount, today) = False Then
											logHandle.WriteLine(Date.Now().ToString("G") & " SQL LINE: " & LineNo.ToString() & " Could not update audit record: " & results)
											logHandle.Flush()
										End If
									End If
								
								End If

								GoTo TOP
							End If
						End If
					End If
				End While
				' we will never get here due to our GOTO logic
#End Region
			Case "IF"
#Region "IF"
				'-----------------------------------
				' I F   C o m m a n d
				'-----------------------------------

				' find out what we are testing for
				Select Case Params(1)
					Case "CPU_VERSION"
				#Region "CPU_VERSION"
						Dim currentVersion As New Version(thisCPUVersion)
						Dim checkVersion As New Version(Params(2))

						' check to see if we are smaller than the checked version
						If currentVersion < checkVersion Then
							logHandle.WriteLine(Date.Now().ToString("G") & " LINE: " & LineNo.ToString() & " Skipping Block of code due to Version too Low. Needed: " & Params(2) & " | Current: " & thisCPUVersion)
							logHandle.Flush()
							canExecuteLine = False
						End If
				#End Region
					Case Else
						logHandle.WriteLine(Date.Now().ToString("G") & " LINE: " & LineNo.ToString() & " [" & Params(1) & "] is not a valid IF case at this moment. Skipping Block of code.")
						logHandle.Flush()
						canExecuteLine = False
				End Select
#End Region
			Case "LOOPIN"
#Region "LOOPIN"
				'-------------------------------
				' L O O P I N  C o m m a n d
				'-------------------------------
				Dim complete As Boolean = False

				'Sets the tempStateInteger to the state integer that we are wanting to check for.
				Dim mode As String = Params(2)
				Dim hours As Integer = Params(3)
				Dim minutes As Integer = Params(4)
				Dim seconds As Integer = Params(5)

				' set the end time from now based off of what we passed in
				Dim endTime As Date = Date.Now().AddHours(hours).AddMinutes(minutes).AddSeconds(seconds)

				Select Params(1)
				    Case "STATE"
			#Region "STATE"
						Dim tempStateInteger As UShort = GetStateIntegerFromStateString(mode)

						Do While Date.Now() < endTime
							' check to see if we got the exit command
							If thisServerInfo.ExitCommand = 1 Then
								GoTo DONE
							End If

							' delay for a second
							Thread.Sleep(MILLISECONDS_IN_SECONDS)

							retval = mbServerClass.GetWord(MODBUS_CurrentStateID(systemIndex), CurrentState)
							If retval <> 0 Then
								Dim message As String = "Could not get System State: (rc=0x" & retval.ToString("X") & ")"
								LogError(mbServerClass, LineNo, message)
								Continue Do
							Else
								DisplayCurrentState()
							End If
				
							' check to see if we have changed states. We are expecting not to change
							If tempStateInteger <> CurrentState Then
								Dim currentStateSting As String = GetStateStringFromStateInteger(CurrentState)
								Dim message As String = "State was changed when not expected. Expected: " & mode & " Changed to: " & currentStateSting
								LogError(mbServerClass, LineNo, message)
								Exit Do
							End If
						Loop
			#End Region
					Case "TASK"
			#Region "TASK"
						Dim tempTaskInteger As UShort = GetTaskIntegerFromTaskString(mode)
						Dim currentTask As Integer = ""

						Do While Date.Now() < endTime
							' check to see if we got the exit command
							If thisServerInfo.ExitCommand = 1 Then
								GoTo DONE
							End If

							' delay for a second
							Thread.Sleep(MILLISECONDS_IN_SECONDS)

							retval = mbServerClass.GetWord(MODBUS_CurrentTaskID(systemIndex), currentTask)
							If retval <> 0 Then
								Dim message As String = "Could not get System Task: (rc=0x" & retval.ToString("X") & ")"
								LogError(mbServerClass, LineNo, message)
								Continue Do
							Else
								'DisplayCurrentTask()
							End If
				
							' check to see if we have changed states. We are expecting not to change
							If tempTaskInteger <> currentTask Then
								Dim currentTaskSting As String = GetTaskStringFromTaskInteger(currentTask)
								Dim message As String = "Task was changed when not expected. Expected: " & mode & " Changed to: " & currentTaskSting
								LogError(mbServerClass, LineNo, message)
								Exit Do
							End If
						Loop
			#End Region
					Case Else
			#Region "Else"
						Dim mbAddress As Integer = Params(1)

						Dim tempValue As UShort = 0

						Do While Date.Now() < endTime
							' check to see if we got the exit command
							If thisServerInfo.ExitCommand = 1 Then
								GoTo DONE
							End If

							' delay for a second
							Thread.Sleep(MILLISECONDS_IN_SECONDS)

							retval = mbServerClass.GetWord(mbAddress, tempValue)
							If retval <> 0 Then
								Dim message As String = "Could not get Address [" & mbAddress & "] Value: (rc=0x" & retval.ToString("X") & ")"
								LogError(mbServerClass, LineNo, message)
								Continue Do
							End If
				
							' check to see if we have changed states. We are expecting not to change
							If tempValue.ToString() <> mode Then
								Dim message As String = "Address [" & mbAddress & "] Value was changed when not expected. Expected: " & mode & " Changed to: " & tempValue
								LogError(mbServerClass, LineNo, message)
								Exit Do
							End If
						Loop
			#End Region
				End Select
#End Region
			Case "LOOPTILL"
#Region "LOOPTILL"
				'-------------------------------
				' L O O P T I L L  C o m m a n d
				'-------------------------------
				Dim complete As Boolean = False

				' sets the tempStateInteger to the state integer that we are wanting to check for.
				Dim mode As String = Params(2)
				Dim hours As Integer = Params(3)
				Dim minutes As Integer = Params(4)
				Dim seconds As Integer = Params(5)

				' set the end time from now based off of what we passed in
				Dim endTime As Date = Date.Now().AddHours(hours).AddMinutes(minutes).AddSeconds(seconds)

				Select Params(1)
					Case "STATE"
			#Region "STATE"
						Dim tempStateInteger As UShort = GetStateIntegerFromStateString(mode)

						Do While Date.Now() < endTime
							' check to see if we got the exit command
							If thisServerInfo.ExitCommand = 1 Then
								GoTo DONE
							End If

							' delay for a second
							Thread.Sleep(MILLISECONDS_IN_SECONDS)

							retval = mbServerClass.GetWord(MODBUS_CurrentStateID(systemIndex), CurrentState)
							If retval <> 0 Then
								Dim message As String = "Could not get System State: (rc=0x" & retval.ToString("X") & ")"
								LogError(mbServerClass, LineNo, message)
							Else
								DisplayCurrentState()
							End If

#If Debug = 1 
							Dim dateStamp As String = Date.Now().ToString("G")
							Dim sh As UInteger = 0
							mbServerClass.GetDword(41208, sh)

							Dim st As UInteger = 0
							mbServerClass.GetDword(43000, st)

							Dim lps As UInteger = 0
							mbServerClass.GetDword(41214, lps)

							Dim dc As UShort = 0
							mbServerClass.GetDword(41007, dc)

							Dim im As UShort = 0
							mbServerClass.GetWord(40000, im)

							logHandle.WriteLine()
							logHandle.WriteLine("---- " & dateStamp & " ----")
							logHandle.WriteLine("System Health:   " & sh)
							logHandle.WriteLine("Input Memory:    " & im)
							logHandle.WriteLine("System Time:     " & st)
							logHandle.WriteLine("Last pump start: " & lps)
							logHandle.WriteLine("Down Counter:    " & dc)
							logHandle.WriteLine()

							logHandle.WriteLine("Current State:   " & CurrentState)
							logHandle.Flush()
#End If
				
							If tempStateInteger = CurrentState Then
								complete = True
								Exit Do
							End If
						Loop

						If complete = False Then
							Dim currentStateSting As String = GetStateStringFromStateInteger(CurrentState)
							Dim message As String = "Time expired for LOOPTILL STATE: " & hours & ":" & minutes & ":" & seconds & " looking for " & mode & " Current = " & currentStateSting
							LogError(mbServerClass, LineNo, message)
						End If
			#End Region
				    Case "TASK"
			#Region "TASK"
						Dim tempTaskInteger As UShort = GetTaskIntegerFromTaskString(mode)
						Dim currentTask As Integer = ""

						Do While Date.Now() < endTime
							' check to see if we got the exit command
							If thisServerInfo.ExitCommand = 1 Then
								GoTo DONE
							End If

							' delay for a second
							Thread.Sleep(MILLISECONDS_IN_SECONDS)

							retval = mbServerClass.GetWord(MODBUS_CurrentTaskID(systemIndex), currentTask)
							If retval <> 0 Then
								Dim message As String = "Could not get System Task: (rc=0x" & retval.ToString("X") & ")"
								LogError(mbServerClass, LineNo, message)
							Else
								'DisplayCurrentTask()
							End If
				
							If tempTaskInteger = currentTask Then
								complete = True
								Exit Do
							End If
						Loop
			
						If complete = False Then
							Dim currentTaskString As String = GetTaskStringFromTaskInteger(currentTask)
							Dim message As String = "Time expired for LOOPTILL TASK: " & hours & ":" & minutes & ":" & seconds & " looking for " & mode & " Current = " & CurrentTaskString
							LogError(mbServerClass, LineNo, message)
						End If
			#End Region
					Case Else
			#Region "Else"
						Dim mbAddress As Integer = Params(1)

						Dim tempValue As UShort = 0

						Do While Date.Now() < endTime
							' check to see if we got the exit command
							If thisServerInfo.ExitCommand = 1 Then
								GoTo DONE
							End If

							' delay for a second
							Thread.Sleep(MILLISECONDS_IN_SECONDS)

							retval = mbServerClass.GetWord(mbAddress, tempValue)
							If retval <> 0 Then
								Dim message As String = "Could not get Address [" & mbAddress & "] Value: (rc=0x" & retval.ToString("X") & ")"
								LogError(mbServerClass, LineNo, message)
							End If
				
							If tempValue.ToString() = mode Then
								complete = True
								Exit Do
							End If
						Loop

						If complete = False Then
							Dim currentStateSting As String = GetStateStringFromStateInteger(CurrentState)
							Dim message As String = "Time expired for LOOPTILL ADDRESS: " & hours & ":" & minutes & ":" & seconds & " looking for " & mode & " Current = " & currentStateSting
							LogError(mbServerClass, LineNo, message)
						End If
			#End Region
				End Select			
#End Region
			Case "MB_WRITE_TEST"
#Region "MB_WRITE_TEST"
				'-------------------------------
				' M B   W R I T E   T E S T   C o m m a n d
				'-------------------------------
				Dim modbusWordAddress As Integer = 3571
				Dim modbusWordValue As Single = 0
				Dim testWordValue As Single = 600

				Dim modbusFloatAddress As Integer = 7830
				Dim modbusFloatValue As Single = 0
				Dim testFloatValue As Single = 18.0

				Dim delay As Integer = Params(1)

				DisplayCurrentState()

				Do Until thisServerInfo.ExitCommand = 1
					'If thisSerialNumber <> "no" And Bypass = False Then
					'	sqlapi.BurnInAudit(myConn, thisSerialNumber, UserName, "", LoopCount, errorCount, today)
					'End If

					DisplayLastTime()
					DisplayErrorCount()
					DisplayLoopCount()

					'Delay for a second
					Thread.Sleep(delay)

			#Region "WRITE FLOAT TEST"
					'****************Used to reset due to getfloat bug.
					modbusFloatAddress = 7830
					modbusFloatValue = 0
					testFloatValue = 18.0

					' write 18.0
					retval = mbServerClass.SetSingle(modbusFloatAddress, testFloatValue)
					If retval <> 0 Then
						Dim message As String = "Could not set Float[" & modbusFloatAddress & "] value: " & testFloatValue & " (rc=0x" & retval.ToString("X") & ")"
						LogError(mbServerClass, 1, message)
					End If

					' get the value
					retval = mbServerClass.GetFloat(modbusFloatAddress, modbusFloatValue)
					If retval <> 0 Then
						Dim message As String = "Could not get Float[" & modbusFloatAddress & "] value: (rc=0x" & retval.ToString("X") & ")"
						LogError(mbServerClass, 2, message)
					End If

					' should be 18.0
					If modbusFloatValue <> testFloatValue Then
						Dim message As String = "Float was changed: Wrote " & testFloatValue & " and read " & modbusFloatValue
						LogError(mbServerClass, 3, message)
					End If

					'***********Used to reset due to getfloat bug.
					modbusFloatAddress = 7830
					modbusFloatValue = 0
					testFloatValue = 16.0

					' write 16.0
					retval = mbServerClass.SetSingle(modbusFloatAddress, testFloatValue)
					If retval <> 0 Then
						Dim message As String = "Could not set Float[" & modbusFloatAddress & "] value: " & testFloatValue & " (rc=0x" & retval.ToString("X") & ")"
						LogError(mbServerClass, 4, message)
					End If

					' get the value
					retval = mbServerClass.GetFloat(modbusFloatAddress, modbusFloatValue)
					If retval <> 0 Then
						Dim message As String = "Could not get Float[" & modbusFloatAddress & "] value: (rc=0x" & retval.ToString("X") & ")"
						LogError(mbServerClass, 5, message)
					End If

					' should still be 16.0
					If modbusFloatValue <> testFloatValue Then
						Dim message As String = "Float was changed: Wrote " & testFloatValue & " and read " & modbusFloatValue
						LogError(mbServerClass, 6, message)
					End If
			#End Region

			#Region "WRITE WORD TEST"
					modbusWordValue = 0
					testWordValue = 600

					' write 600
					retval = mbServerClass.SetWord(modbusWordAddress, testWordValue)
					If retval <> 0 Then
						Dim message As String = "Could not set Word[" & modbusWordAddress & "] value: " & testWordValue & " (rc=0x" & retval.ToString("X") & ")"
						LogError(mbServerClass, 7, message)
					End If

					' get the value
					retval = mbServerClass.GetWord(modbusWordAddress, modbusWordValue)
					If retval <> 0 Then
						Dim message As String = "Could not get Word[" & modbusWordAddress & "] value: (rc=0x" & retval.ToString("X") & ")"
						LogError(mbServerClass, 8, message)
					End If

					' should still be 600
					If modbusWordValue <> testWordValue Then
						Dim message As String = "Word was changed: Wrote " & testWordValue & " and read " & modbusWordValue
						LogError(mbServerClass, 9, message)
					End If

					modbusWordValue = 0
					testWordValue = 60

					' write 60
					retval = mbServerClass.SetWord(modbusWordAddress, testWordValue)
					If retval <> 0 Then
						Dim message As String = "Could not set Word[" & modbusWordAddress & "] value: " & testWordValue & " (rc=0x" & retval.ToString("X") & ")"
						LogError(mbServerClass, 10, message)
					End If

					' get the value
					retval = mbServerClass.GetWord(modbusWordAddress, modbusWordValue)
					If retval <> 0 Then
						Dim message As String = "Could not get Word[" & modbusWordAddress & "] value: (rc=0x" & retval.ToString("X") & ")"
						LogError(mbServerClass, 11, message)
					End If

					' should still be 60
					If modbusWordValue <> testWordValue Then
						Dim message As String = "Word was changed: Wrote " & testWordValue & " and read " & modbusWordValue
						LogError(mbServerClass, 12, message)
					End If
			#End Region
					LoopCount += 1
				Loop
				GoTo DONE
#End Region
			Case "MODE"
#Region "MODE"
				'-----------------------------------
				' M O D E   C o m m a n d
				'-----------------------------------
				Dim task As String = Params(1)
				Dim SendCommand As Integer = GetTaskIntegerFromTaskString(task)

				' if we have a valid command to send, proceed
				If SendCommand <> 0 Then
					
					retval = mbServerClass.SetWord(MODBUS_InputTaskCommand(systemIndex), SendCommand)
					If retval <> 0 Then
						Dim message As String = "Could not set System Command: " & task & " (rc=0x" & retval.ToString("X") & ")"
						LogError(mbServerClass, LineNo, message)
					End If
				Else
					' command is not valid, log what it is
					Dim message As String = "Command: " & task & " not allowed."
					LogError(mbServerClass, LineNo, message)
				End If
#End Region
			Case "PRINT"
#Region "PRINT"
				'-----------------------------------
				' P R I N T   C o m m a n d
				'-----------------------------------

				Dim outString As String = ""
				Dim inString As Boolean = False

				' start offset by 6 so we do not have [PRINT "]
				Dim printLine = curLine.Substring(6)
				Dim printsep(1) As Char
				printsep(0) = """"

				' split out the parameters in a custom way, because print needs it
				Params = printLine.Split(printsep, 10)

				For index As Integer = 1 To Params.Length - 1

					If Params(index).Length <> 0 Then
						Dim thisStart As Integer = 0
						Dim startVar As Integer

						' step through each character and see if we are calling out a custom VAR
						For startVar = 1 To Params(index).Length - 1
							If GetChar(Params(index), startVar) = "$" Then
								thisStart = startVar
								Exit For
							End If
						Next

						If 0 < startVar And GetChar(Params(index), startVar) = CChar("$") Then
							For vindex As Integer = 0 To lastTableIndex - 1
								If Params(index).Substring(thisStart - 1) = vTable(vindex).name Then
									outString &= vTable(vindex).value.ToString()
									Exit For
								End If
							Next
						Else
							outString &= Params(index) & " "
						End If
					End If
				Next

				currentTest = outString
				DisplayCurrentTest()

				logHandle.WriteLine(Date.Now().ToString("G") & "  " & outString)
				logHandle.Flush()
#End Region
			Case "RECONNECT"
#Region "RECONNECT"
				'-----------------------------------
				' R E C O N N E C T
				'-----------------------------------
				Dim complete As Boolean = False
				Dim hours As Integer = Params(1)
				Dim minutes As Integer = Params(2)
				Dim seconds As Integer = Params(3)

				' set the end time from now based off of what we passed in
				Dim endTime As Date = Date.Now().AddHours(hours).AddMinutes(minutes).AddSeconds(seconds)

				' set our current state to "Reseting"
				CurrentState = STATE_Reseting(systemIndex)
				DisplayCurrentState()

				Dim responseData As String = ""
				Dim message As String = ""

				Dim restart As Boolean = False

				' try to reset the system 3 times
				For i As Integer = 1 To 3 Step 1
					If jsonapi.DoRemoteReboot(thisServerInfo.ServerName, thisModel, responseData) = False Then
						message = "Could not make the system restart: " & vbNewLine & 
							responseData

						' give some breathing room
						Thread.Sleep(MILLISECONDS_IN_SECONDS * 5)
					Else
						restart = True
						Exit For
					End If
				Next

				If restart = False Then
					LogError(mbServerClass, LineNo, message)
					GoTo TOP
				End If

				logHandle.WriteLine(Date.Now().ToString("G") & " LINE: " & LineNo.ToString() & " System restart: " & responseData)
				logHandle.Flush()

				mbServerClass.CloseHandle()

				' give the box enough time to shut down before we try to reconnect
				Thread.Sleep(MILLISECONDS_IN_SECONDS * 10)

				' set our current state to "Reconnecting"
				CurrentState = STATE_Reconnecting(systemIndex)
				DisplayCurrentState()

				Do While Date.Now() < endTime
					' first, delay for a second
					Thread.Sleep(MILLISECONDS_IN_SECONDS)

					' try to connect
					If (0 <> mbServerClass.OpenTCP(thisServerInfo.ServerName, 502)) Then
						complete = True
						Exit Do
					End If

					' check for the kill switch
					If thisServerInfo.ExitCommand = 1 Then
						GoTo DONE
					End If
				Loop

				' check to see if we connected
				If complete = False Then
					' set our current state to "Could Not Reconnect"
					CurrentState = STATE_CouldNotReconnect(systemIndex)
					message = "Could not reconnect to the System after " & hours & ":" & minutes & ":" & seconds
					LogError(mbServerClass, LineNo, message)

					DisplayCurrentState()
					DisplayErrorCount()

					GoTo DONE
				End If

				logHandle.WriteLine(Date.Now().ToString("G") & " LINE: " & LineNo.ToString() & " System Reconnected.")
				logHandle.Flush()

				thread.Sleep(10000)
#End Region
			Case "REMOTE"
#Region "REMOTE"
				'-----------------------------------
				' R E M O T E   C o m m a n d 
				'-----------------------------------
				Dim diValue As Integer
				Dim canContinue As Boolean = True

				' load up the current simulated digin values so we have them
				retval = mbServerClass.GetWord(MODBUS_SimulatedDigitalIn(systemIndex), diginValues)

				' check to see if we support the command
				If (Params(1) = "START") Then
					' check to see if the bit is off
					If (diginValues And DI_Start(systemIndex)) = DI_Start(systemIndex) Then
						logHandle.WriteLine(Date.Now().ToString("G") & " WARNING LINE: " & LineNo.ToString() & " Start bit is already high")
						logHandle.Flush()
					End If

					' turn the bit on
					diValue = diginValues Or OnMask(DI_Start(systemIndex) - 1)
				ElseIf (Params(1) = "STOP") Then
					' check to see if the bit is off
					If (diginValues And DI_Stop(systemIndex)) <> DI_Stop(systemIndex) Then
						logHandle.WriteLine(Date.Now().ToString("G") & " WARNING LINE: " & LineNo.ToString() & " Stop bit is already low")
						logHandle.Flush()
					End If

					' turn the bit off
					diValue = diginValues And OffMask(DI_Stop(systemIndex) - 1)
				Else
					Dim message As String = "Invalid remote command: " & Params(1)
					LogError(mbServerClass, LineNo, message)
					canContinue = False
				End If

				' check to see that we can continue with the command
				If canContinue = True Then

					' turn the value on
					retval = mbServerClass.SetWord(MODBUS_SimulatedDigitalIn(systemIndex), diValue)
					If retval <> 0 Then
						Dim message As String = "Could not set Digital value: (rc=0x" & retval.ToString("X") & ")"
						LogError(mbServerClass, LineNo, message)
					End If

					' wait some time
					thread.Sleep(MILLISECONDS_IN_SECONDS)

					' turn the value off
					retval = mbServerClass.SetWord(MODBUS_SimulatedDigitalIn(systemIndex), diginValues)
					If retval <> 0 Then
						Dim message As String = "Could not set Digital value: (rc=0x" & retval.ToString("X") & ")"
						LogError(mbServerClass, LineNo, message)
					End If
				End If
#End Region
			Case "SCHEDULE"
#Region "SCHEDULE"
				'-------------------------------
				' S C H E D U L E  C o m m a n d
				'-------------------------------
				Dim systemCurrentTime_Seconds As Integer = 0

				' get the SYSTEM current time
				retval = mbServerClass.GetDword(MODBUS_SystemTime(systemIndex), systemCurrentTime_Seconds)
				If retval <> 0 Then
					Dim message As String = "Could not get the System Time: (rc=0x" & retval.ToString("X") & ")"
					LogError(mbServerClass, LineNo, message)
				Else
					' start with the current time, EST subtract 5 hours, and no daylight savings
					Dim systemCurrentTime_Date As Date = DateAdd("s", (systemCurrentTime_Seconds - (5 * SECONDS_IN_HOUR)), "1/1/1970 00:00:00")

					' check to see if we are in daylight savings
					If TimeZoneInfo.Local.IsDaylightSavingTime(Date.Now()) = True Then
						' subtract 4 hours in seconds to account for daylight savigns
						systemCurrentTime_Date = DateAdd("s", (systemCurrentTime_Seconds - (4 * SECONDS_IN_HOUR)), "1/1/1970 00:00:00")
					End If

					' add 1 at the end to make sure that we are going to hit the schedule
					Dim systemAfterMidnight_Minutes As Integer = (systemCurrentTime_Date.Hour * MINUTES_IN_HOUR) + systemCurrentTime_Date.Minute + 1

					' find out what schedule we are creating
					Select Case Params(1)
						Case "BUMP", "DRAIN", "PRECOAT"
			#Region "SCHEDULE"
							Dim timeAddress As Integer = 0
							Dim dayAddress As Integer = 0

							' set the correct modbus address for the time and day
							If Params(1) = "BUMP" Then
								dayAddress = MODBUS_BumpScheduleDays(systemIndex)
								timeAddress = MODBUS_BumpScheduleTime(systemIndex)

								mbServerClass.SetWord(MODBUS_BumpScheduleEnable(systemIndex), 1)
							ElseIf Params(1) = "DRAIN" Then
								dayAddress = MODBUS_DrainScheduleDays(systemIndex)
								timeAddress = MODBUS_DrainScheduleTime(systemIndex)

								mbServerClass.SetWord(MODBUS_DrainScheduleEnable(systemIndex), 1)
							ElseIf Params(1) = "PRECOAT" Then
								dayAddress = MODBUS_PrecoatScheduleDays(systemIndex)
								timeAddress = MODBUS_PrecoatScheduleTime(systemIndex)

								mbServerClass.SetWord(MODBUS_PrecoatScheduleEnable(systemIndex), 1)
							End If

							Dim ScheduleDay As Integer = systemCurrentTime_Date.DayOfWeek

							' check to see if we will roll over into a new day
							If MINUTES_IN_DAY < = systemAfterMidnight_Minutes + Params(2) Then

								' subtract a whole day in minutes from the minutes after midnight to get the correct time after midnight
								systemAfterMidnight_Minutes = systemAfterMidnight_Minutes - MINUTES_IN_DAY

								' adjust the schedule day roll over
								If ScheduleDay = DayOfWeek.Saturday Then
									ScheduleDay = DayOfWeek.Sunday
								Else
									ScheduleDay += 1
								End If
							End If

							' try to set the time for the schedule
							retval = mbServerClass.SetWord(timeAddress, (systemAfterMidnight_Minutes + Params(2)))
							If retval <> 0 Then
								Dim message As String = "Could not set " & Params(1) & " start time (rc=0x" & retval.ToString("X") & ")"
								LogError(mbServerClass, LineNo, message)
							Else
								logHandle.WriteLine(Date.Now().ToString("G") & " LINE: " & LineNo.ToString() & " Set " & Params(1) & " time for: " & (systemAfterMidnight_Minutes + Params(2)))
								logHandle.Flush()
							End If

							' get the bitmask for the day of the week and set the day bitmask for the schedule
							retval = mbServerClass.SetWord(dayAddress, DayToBitmask(ScheduleDay))
							If retval <> 0 Then
								Dim message As String = "Could not set " & Params(1) & " day bitmask (rc=0x" & retval.ToString("X") & ")"
								LogError(mbServerClass, LineNo, message)
							End If
			#End Region
						Case "BLOCK"
			#Region "BLOCK"
							'
							' first we need to set the block start time
							'

							mbServerClass.SetWord(MODBUS_StartBlockEnabled(systemIndex), 1)

							Dim ScheduleDay As Integer = systemCurrentTime_Date.DayOfWeek

							dim startBlock As integer = systemAfterMidnight_Minutes - 5 

							' check to see if we just rolled over into a new day
							If startBlock < 0 Then
								' make the time 23:55
								startBlock = MINUTES_IN_DAY - 5

								' adjust the schedule day roll over
								If ScheduleDay = DayOfWeek.Sunday Then
									ScheduleDay = DayOfWeek.Saturday
								Else
									ScheduleDay -= 1
								End If
							End If
						
							' try to set the time for the block
							retval = mbServerClass.SetWord(MODBUS_StartBlockScheduleTime(systemIndex), startBlock)
							If retval <> 0 Then
								Dim message As String = "Could not set Bump Block start time (rc=0x" & retval.ToString("X") & ")"
								LogError(mbServerClass, LineNo, message)
							Else
								logHandle.WriteLine(Date.Now().ToString("G") & " LINE: " & LineNo.ToString() & " Set Bump Block start time for: " & startBlock)
								logHandle.Flush()
							End If

							' get the bitmask for the day of the week and set the day bitmask for the schedule
							retval = mbServerClass.SetWord(MODBUS_StartBlockScheduleDays(systemIndex), DayToBitmask(ScheduleDay))
							If retval <> 0 Then
								Dim message As String = "Could not set Bump Block start day bitmask (rc=0x" & retval.ToString("X") & ")"
								LogError(mbServerClass, LineNo, message)
							End If

							'
							' now we need to set the end of the block
							'

							mbServerClass.SetWord(MODBUS_StopEnabledRegister(systemIndex), 1)

							ScheduleDay = systemCurrentTime_Date.DayOfWeek

							systemAfterMidnight_Minutes += Params(2)

							' check to see if we will roll over into a new day
							If MINUTES_IN_DAY <= systemAfterMidnight_Minutes Then

								' subtract a whole day in minutes from the minutes after midnight to get the correct time after midnight
								systemAfterMidnight_Minutes = systemAfterMidnight_Minutes - MINUTES_IN_DAY

								' adjust the schedule day roll over
								If ScheduleDay = DayOfWeek.Saturday Then
									ScheduleDay = DayOfWeek.Sunday
								Else
									ScheduleDay += 1
								End If
							End If

							' try to set the time for the end
							retval = mbServerClass.SetWord(MODBUS_StopBlockScheduleTime(systemIndex), systemAfterMidnight_Minutes)
							If retval <> 0 Then
								Dim message As String = "Could not set Bump Block stop time (rc=0x" & retval.ToString("X") & ")"
								LogError(mbServerClass, LineNo, message)
							Else
								logHandle.WriteLine(Date.Now().ToString("G") & " LINE: " & LineNo.ToString() & " Set Bump Block stop time for: " & systemAfterMidnight_Minutes)
								logHandle.Flush()
							End If

							' get the bitmask for the day of the week and set the day bitmask for the end
							retval = mbServerClass.SetWord(MODBUS_StopBlockScheduleDays(systemIndex), DayToBitmask(ScheduleDay))
							If retval <> 0 Then
								Dim message As String = "Could not set Bump Block stop day bitmask (rc=0x" & retval.ToString("X") & ")"
								LogError(mbServerClass, LineNo, message)
							End If
			#End Region
					End Select
				End If
#End Region
			Case "SETDWORD"
#Region "SETDWORD"
				'-----------------------------------
				' S E T D W O R D   C o m m a n d
				'-----------------------------------
				Dim MBAddress As Integer
				Dim tempDword As UInteger
				MBAddress = CInt(Params(1))
				tempDword = CUInt(Params(2))

				retval = mbServerClass.SetDword(MBAddress, tempDword)
				If retval <> 0 Then
					Dim message As String = "Could not set Double Word value: " & Params(1) & " (rc=0x" & retval.ToString("X") & ")"
					LogError(mbServerClass, LineNo, message)
				End If
#End Region
			Case "SETFLOAT"
#Region "SETFLOAT"
				'-----------------------------------
				' S E T F L O A T   C o m m a n d
				'-----------------------------------
				Dim MBAddress As Integer
				Dim tempFloat As Single
				MBAddress = CInt(Params(1))
				tempFloat = CSng(Params(2))

				retval = mbServerClass.SetSingle(MBAddress, tempFloat)
				If retval <> 0 Then
					Dim message As String = "Could not set Float value: " & Params(1) & " (rc=0x" & retval.ToString("X") & ")"
					LogError(mbServerClass, LineNo, message)
				End If
#End Region
			Case "SETPARAM"
#Region "SETPARAM"
				'-----------------------------------
				' S E T P A R A M   C o m m a n d 
				'-----------------------------------
				Dim paramNumber As Integer = Params(1)
				Dim paramValue As Integer = Params(2)

				' check to see that we are in the correct range of parameters
				If (paramNumber < 1 Or maxParameters(systemIndex) < paramNumber) Then
					Dim message As String = "Invalid Parameter number: " & paramNumber
					LogError(mbServerClass, LineNo, message)
				Else
					' try to set the parameter (offset by one because the Parameter Base is parameter 1)
					retval = mbServerClass.SetWord(MODBUS_ProcessParameters(systemIndex) + (paramNumber - 1), paramValue)
					If retval <> 0 Then
						Dim message As String = "Could not set Parameter" & paramNumber & " to value: " & paramValue & " (rc=0x" & retval.ToString("X") & ")"
						LogError(mbServerClass, LineNo, message)
					End If
				End If
#End Region
			Case "SETWORD"
#Region "SETWORD"
				'-----------------------------------
				' S E T W O R D   C o m m a n d
				'-----------------------------------
				Dim MBAddress As Integer
				Dim tempWord As UShort
				MBAddress = CInt(Params(1))
				tempWord = CUShort(Params(2))

				retval = mbServerClass.SetWord(MBAddress, tempWord)
				If retval <> 0 Then
					Dim message As String = "Could not set Word value: " & Params(1) & " (rc=0x" & retval.ToString("X") & ")"
					LogError(mbServerClass, LineNo, message)
				End If
#End Region
			Case "SIMULATEIO"
#Region "SIMULATEIO"
			'-----------------------------------
			' S I M U L A T E I O   C o m m a n d
			'-----------------------------------
			retval = mbServerClass.SetWord(MODBUS_ProcessParameters(systemIndex) + RUNTIME_INPUT_OVERRIDE(systemIndex), 1)

			If retval <> 0 Then
					Dim message As String = "Could not set System into Simulate IO (rc=0x" & retval.ToString("X") & ")"
					LogError(mbServerClass, LineNo, message)
			Else
				' load up the simulated digin values so we have them
				retval = mbServerClass.GetWord(MODBUS_SimulatedDigitalIn(systemIndex), diginValues)
			End If
#End Region
			Case "VAR"
#Region "VAR"
				'-----------------------------------
				' V A R   C o m m a n d
				'-----------------------------------

				' add the name to the table and initilize the value
				vTable(lastTableIndex).name = Params(1)
				vTable(lastTableIndex).value = 0

				' check to see if we are assigning it a value
				If Params(2) = "=" Then
					vTable(lastTableIndex).value = CInt(Params(3))
				End If
				lastTableIndex += 1
#End Region
			Case Else
				If GetChar(curLine, 1) = CChar(":") Then
#Region ":"
					'-----------------------------------
					' L A B E L   D E C L A R A T I O N
					'-----------------------------------
					'Do nothing for label declarations
#End Region
				Else
#Region "VARIABLES"
					'-----------------------------------
					' V A R I A B L E   C o m m a n d
					'-----------------------------------
					Dim foundVar As Boolean = False

					' lookup variable name in variable name/value table
					For index As Integer = 0 To lastTableIndex - 1

						' check to see if we have a match
						If commandString = vTable(index).name Then

							' check to see if we are 'adding' or 'subtracting'
							If Params(1) = "+=" Then
								vTable(index).value += CInt(Params(2))
							ElseIf Params(1) = "-=" Then
								vTable(index).value -= CInt(Params(2))
							End If

							foundVar = True
							Exit For
						End If
					Next

					' check to see if we have not found any result
					If foundVar = False Then
						Dim message As String = "Word is not defined anywhere: " & commandString
						LogError(mbServerClass, LineNo, message)
					End If
#End Region
				End If
		End Select

		GoTo TOP
DONE:

		logHandle.WriteLine(Date.Now().ToString("G") & " FINISHED.")
		logHandle.WriteLine("Loops: " & LoopCount)
		logHandle.WriteLine("Errors: " & errorCount)
		logHandle.WriteLine("")

		fileHandle.Close()
		logHandle.Close()

DONE_NF:
		thisServerInfo.IPStable = 1
		DisplayIndicator()
		DisplayIndicatorT()
		mbServerClass.CloseHandle()
		thisServerInfo.ExitCommand = 0
		thisServerInfo.ServerName &= " closed"
		
		
		' dispaly messages if the error is before we open our text file to prevent the system from hanging during setup.
		If errorMessage.Length <> 0 Then
			Dim cmb As New CustomMsgBox(errorMessage)
			cmb.ShowDialog
		End If
	End Sub

	Private function DayToBitmask(byref dayInteger As Integer) As Integer
		Dim retval = 0

		Select Case dayInteger
			Case DayOfWeek.Sunday
				retval = SUNDAY
			Case DayOfWeek.Monday
				retval = MONDAY
			Case DayOfWeek.Tuesday
				retval = TUESDAY
			Case DayOfWeek.Wednesday
				retval = WEDNESDAY
			Case DayOfWeek.Thursday
				retval = THURSDAY
			Case DayOfWeek.Friday
				retval = FRIDAY
			Case DayOfWeek.Saturday
				retval = SATURDAY
		End Select

		Return retval
	End function

	Private sub LogError(byref mbServerClass As ModbusXfce, ByRef LineNo As Integer, byref message As string)
		Dim SystemHealth As Integer = 0
		dim retval = mbServerClass.GetDword(MODBUS_SystemHealth(systemIndex), SystemHealth)

		If retval <> 0 Then

		End If

		Dim dateStamp As String = Date.Now().ToString("G")
		logHandle.WriteLine(dateStamp & " ERROR LINE: " & LineNo & " " & message)

		logHandle.WriteLine()
		logHandle.WriteLine("----- Data Dump -----")
		logHandle.WriteLine("System Health: " & SystemHealth)
		logHandle.WriteLine("RC: " & retval)
		logHandle.WriteLine()

		logHandle.Flush()

		mbServerClass.LogMessage(dateStamp & " ERROR LINE: " & LineNo)' & vbNewLine)

		If errorCount = 0 Then
			DisplayIndicator()
			DisplayIndicatorT()
		End If
		errorCount += 1
	End sub

End Class