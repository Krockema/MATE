using Master40.DB.Data.Context;
using Master40.DB.GanttplanDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master40.DB.Data.Initializer
{
    public class GPSzenarioInitializer
    {
        public static void DbInitialize(GPSzenarioContext context)
        {
            context.Database.EnsureCreated();

            if (context.Config.Any())
                return;

            var configs = new Config[]
            {
                new Config { PropertyName = "TableDesignVersion", Value = "2"},
                new Config { PropertyName = "LicenceInfo", Value = @"LicProtectorVersion: 5.0.0.641
LicFileID: 39656
LicFileVersion: 5000
Customer: HTW Dresden
LPWindowsUser: administrator
LPComputerName: FELIX-PC
RunsOnVirtualMachine: 0
MainModuleTag: DEMO
AvailableEdition: OE
SelectedEdition: OE
Path: C:\ProgramData\DUALIS GmbH IT Solution\GANTTPLAN.lic5
Volume: C:\
Label: Lokaler Datenträger
VolumeID: EEB0-7853
FileSystem: NTFS"},
                new Config { PropertyName = "SystemInfo", Value = @"Windows-Edition: Windows Server 2016 Standard 64-bit
Processor: Intel(R) Core(TM) i5-6400 CPU @ 2.70GHz
Installed memory: 15.89 GB (8.85 GB free - load 44%)

Computer name: IPC7
Domain name: smb.informatik.htw-dresden.de
Login name: IPC7\Administrator"},
                new Config { PropertyName = "GPVersion", Value = "5.7.6 x64 "},
                new Config { PropertyName = "GPDBVersion", Value = "29"},
                new Config { PropertyName = "SQLiteVersion", Value = "3.22.0"},
                new Config { PropertyName = "GANTTPLAN.config", Value = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!-- Copyright © 2007-2018 DUALIS GmbH IT Solution - All Rights Reserved -->
<configuration>

    <version/>

    <default>

        <add key=""opt.default.config""/>

        <add key=""interface.default.config""/>

        <add key=""colors.default.config""/>

        <add key=""icons.default.config""/>

    </default>

    <redirect>

        <add key=""*"" value=""local.config""/>

    </redirect>

    <application>

        <!-- the licence -->

        <!-- PATH_LICENCEFILE: Gibt an, wo die Lizenzdatei liegt-- >
            
                    < !--LICENCE_LOGGING: Gibt an, ob das Logging des Lizensystems angeschalten ist -->
            
                    < !--PATH_LICENCE_LOGGING: Gibt an, wo das Logging des Lizensystems geschieht-- >
            
                    < !--ohne Wert: Es wird unter Dokumente/ Ganttplan gelogged-- >
 
         < !--mit Wert: Es wird in dem angegebenen Ordner im Unterordner \log\< user > _ < host >\ gelogged-- >
       
               < add key = ""PATH_LICENCEFILE"" value = """" />
          
                  < add key = ""LICENCE_LOGGING"" value = ""false"" />
             
                     < add key = ""PATH_LICENCE_LOGGING"" value = """" />
                

                        < !--used font settings -->
                
                        < !--< add key = ""GPFONT_NAME"" value = ""Segoe UI"" /> -->
                    
                            < !--< add key = ""GPFONT_SIZE"" value = ""9"" /> -->
                        

                                < !--use workerthreads for lengthy operations -->
                        
                                < add key = ""USE_WORKERTHREADS"" value = ""true"" />
                        

                                < !--start custom exe on events-- >
                        
                                < !--< add key = ""EXE_CUSTOM_ONINIT"" value = ""false"" file = """" path = """" param = """" /> -->
                        
                                < !--< add key = ""EXE_CUSTOM_ONIMPORTMODEL"" value = ""false"" file = """" path = """" param = """" /> -->
                        
                                < !--< add key = ""EXE_CUSTOM_ONEXIT"" value = ""false"" file = """" path = """" param = """" /> -->
                        
                                < !--< add key = ""EXE_CUSTOM_ONEXPORT"" value = ""false"" file = """" path = """" param = """" /> -->
                        
                                < !--< add key = ""EXE_CUSTOM_ONLIVEUPDATE"" value = ""false"" file = """" path = """" param = """" /> -->
                        
                                < !--< add key = ""EXE_CUSTOM_ONRELOADORDERS"" value = ""false"" file = """" path = """" param = """" /> -->
                        
                                < !--< add key = ""EXE_CUSTOM_ONSYNC_BEFORE"" value = ""false"" file = """" path = """" param = """" /> -->
                        
                                < !--< add key = ""EXE_CUSTOM_ONSYNC_AFTER"" value = ""false"" file = """" path = """" param = """" /> -->
                        
                                < !--< add key = ""EXE_CUSTOM_ONPREPAREDATA"" value = ""false"" file = """" path = """" param = """" /> -->
                        

                                < !--show popup with error message in case the custom exe returns not 0
                                < !--e.g.EXE_CUSTOM_ERRORCODE_INFO_3"" value=""error 3"" if you want to show an error message when your exe returns 3 -->
                                < !-- < add key = ""EXE_CUSTOM_ERRORCODE_INFO_XY"" value = ""..."" /> -->
                        

                                < !--Default - Language - Keys-- >
                        
                                < add key = ""LANGUAGE_DEFAULT"" value = ""DE"" />
                        
                                < add key = ""STRINGTABLE_DE"" value = ""ganttplan_DE.lang|shared_DE.lang"" />
                        
                                < add key = ""STRINGTABLE_EN"" value = ""ganttplan_EN.lang|shared_EN.lang"" />
                        
                                < !--User - Language - Keys-- >
                        
                                < !--You can define your own language files.These files will overwrite values from default language files. -- >
                        
                                < !--Here is an example with language value ""*"".You can replace the value ""*"", e.g.with ""FR"" or ""RUS"" or ""myFile""
                                < add key = ""LANGUAGE"" value = ""*"" />
                        
                                < add key = ""STRINGTABLE_*"" value = ""ganttplan_*.lang"" />
                        

                                < !--Show localized numbers in all grids and controls(localized numbers will respect the current locale settings from Windows (Country / Region and language)-- >
                        
                                < !--true: Use locale settings from Windows -->
                        
                                < !--false: Use neutral language for numerics-- >
                        
                                < add key = ""SHOW_LOCALIZED_NUMERICS"" value = ""true"" />
                        

                                < !--live update interval Time / Status / BDE-- >
                        
                                < add key = ""AUTO_UPDATE_INTERVAL_SEC"" value = ""60"" />
                        
                                < !--visual warnings on plan delay-- >
                        
                                < add key = ""GANTT_WARNING_MIN_PLAN_DELAY_ABS"" value = ""60"" />
                        

                                < !--show comments from calendar-or shift intervals in gantt-- >
                        
                                < !--0: show no comments-- >
                        
                                < !--1: show only comments from not worktime intervals-- >
                        
                                < !--2: show only comments from worktime intervals -->
                        
                                < !--3: show all comments from all interval types-- >
                        
                                < add key = ""GANTT_SHOW_CALENDAR_COMMENTS"" value = ""1"" />
                        
                                < add key = ""GANTT_SHOW_SHIFTINTERVAL_COMMENTS"" value = ""1"" />
                        

                                < !--show process descriptions over bars instead centered in bars-- >
                        
                                < add key = ""GANTT_SHOW_DESCRIPTIONS_ABOVE_BARS"" value = ""false"" />
                        

                                < !--show tooltips in gantt-- >
                        
                                < add key = ""GANTT_SHOW_TOOLTIPS"" value = ""true"" />
                        

                                < !--gantt description identifier-- >
                        
                                < add key = ""GANTT_IDENTIFIER"" value = ""¿"" />
                        
                                < !--gantt description texts-- >
                        
                                < !--add key = ""GANTT_LABEL_WORKCENTER"" value = ""¿workcenter_id¿ / ¿name¿"" / -->
                        
                                < !--add key = ""GANTT_LABEL_WORKCENTERGROUP"" value = ""¿workcentergroup_id¿ / ¿name¿"" / -->
                        
                                < !--add key = ""GANTT_LABEL_WORKER"" value = ""¿worker_id¿ / ¿name¿"" / -->
                        
                                < !--add key = ""GANTT_LABEL_WORKERGROUP"" value = ""¿workergroup_id¿ / ¿name¿"" / -->
                        
                                < !--add key = ""GANTT_LABEL_PRT"" value = ""¿prt_id¿ / ¿name¿"" / -->
                        
                                < !--add key = ""GANTT_LABEL_PRODUCTIONORDER"" value = ""¿productionorder_id¿ / ¿name¿"" / -->
                        
                                < !--add key = ""GANTT_LABEL_PRODUCTIONORDER_OPERATION_ACTIVITY"" value = ""¿productionorder_id¿ / ¿name¿"" / -->
                        

                                < !--sort orders by property(empty value to disable)-- >
                        
                                < add key = ""GANTT_VIEWORDER_SORT"" value = ""productionorder_id"" ascending = ""true"" hierarchy = ""true"" />
                        

                                < !--fill mode for productionorder view -->
                        
                                < !--0: empty at start, it is allowed to add and remove productionorders-- >
                        
                                < !--1: all productionorders are visible, it is not allowed to remove productionorders-- >
                        
                                < !--2: filled with all primary productionorders, it is allowed to add and remove productionsorders, except primary productionorders-- >
                        
                                < add key = ""GANTT_VIEWORDER_FILLMODE"" value = ""0"" />
                        

                                < !--gantt colors for weekday intervals -->
                        
                                < !--key = GANTT_COLOR_WEEKDAY_ + ""Weekday"" as integer -->
                        
                                < !--0 = Monday, 1 = Tuesday, ..., 6 = Sunday-- >
                        
                                < !--e.g coloring all sundays light red: -->
                        
                                < !-- < add key = ""GANTT_COLOR_WEEKDAY_6"" a = ""75"" r = ""255"" g = ""25"" b = ""25"" /> -->
                        

                                < !--gantt colors for info intervals -->
                        
                                < add key = ""GANTT_COLOR_INFO_INTERVAL"" a = ""90"" r = ""255"" g = ""0"" b = ""150"" />
                        

                                < !--gantt color range when creating a random color-- >
                        
                                < add key = ""GANTT_COLOR_RANDOM_MIN"" r = ""0"" g = ""0"" b = ""0"" />
                        
                                < add key = ""GANTT_COLOR_RANDOM_MAX"" r = ""255"" g = ""255"" b = ""255"" />
                        

                                < !--resource schedule colors for interval type -->
                        
                                < !-- if one value from the rgb code is -1, the interval type is not displayed in the resource schedule-- >
                         
                                 < !--key = RESOURCESCHEDULE_COLOR_2 + ""Type"" as integer -->
                         
                                 < !--1 = Pause, 2 = Arbeit, 3 = Urlaub, 4 = Krank, 5 = Wartung, 6 = Störung-- >
                         
                                 < add key = ""RESOURCESCHEDULE_COLOR_2"" r = ""0"" g = ""255"" b = ""0"" />
                                
                                        < add key = ""RESOURCESCHEDULE_COLOR_3"" r = ""30"" g = ""144"" b = ""255"" />
                                       
                                               < add key = ""RESOURCESCHEDULE_COLOR_4"" r = ""148"" g = ""0"" b = ""211"" />
                                              

                                                      < !--add key = ""HIDE_SPLASH"" / -->
                                               
                                                       < !--add key = ""EXPORT_ON_APPLY"" / -->
                                                
                                                        < !--add key = ""BACKUPEXTERN_ON_APPLY"" / -->
                                                 
                                                         < !--add key = ""SILENT"" / -->
                                                  
                                                          < !--add key = ""CLIENT_ID"" / -->
                                                   

                                                           < !--reload all external data before optimization starts-->
                                                   
                                                           < add key = ""STARTOPT_RELOAD_EXTERNAL_DATA"" value = ""false"" />
                                                      

                                                              < !--use custom special release version info instead of original one-- >
                                                      
                                                              < !--< add key = ""CUSTOM_VERSION_INFO"" value = ""MyCustomInfo"" /> -->
                                                          

                                                                  < !--check for new version -- >
                                                          
                                                                  < add key = ""VERSIONCHECK_VERSION"" value = ""true"" />
                                                          
                                                                  < !--check for new beta version(only respected if check for new versions is true) -->
                                                         
                                                                 < add key = ""VERSIONCHECK_BETA"" value = ""false"" />
                                                            
                                                                    < !--check automatically at startup-- >
                                                            
                                                                    < add key = ""VERSIONCHECK_AUTOMATIC"" value = ""true"" />
                                                               
                                                                       < !--check interval in days(0 = check always)-- >
                                                               
                                                                       < add key = ""VERSIONCHECK_AUTOMATIC_INTERVAL"" value = ""1"" />
                                                                  

                                                                          < !--Path to Working Directory-- >
                                                                  
                                                                          < add key = ""PATH_WORKINGDIRECTORY"" value = """" />
                                                                     

                                                                             < !--Changes how images are read and saved -->
                                                                     
                                                                             < !--0:	never read images, never save images-- >
                                                              
                                                                      < !--1:	Prefer loading images from Database(+image)-- >
                                                       
                                                               < !--2: 	Prefer loading images from file(+image_file)-- >
                                                
                                                        < !--For 1 and 2 the bahviour is identical, except that user defined fields(UDF) are prioritized differently-- >
                                               
                                                       < !--The behavior is as following: -->
                                               
                                                       < !---Read from the UDF +image or + image_file-- >
                                       
                                               < !---If both UDFs are filled read from the prefered UDF-- >
                               
                                       < !---Save to the same UDF it was read from -->
                       
                               < !---If no image was read save to the prefered UDF-- >
               
                       < add key = ""RESOURCEIMAGE_MODE"" value = ""1"" />
                  

                      </ application >
                  
                      < database_ganttplan >
                  
                          < !-- do custom sql calls on events -->
                   
                           < !--< add key = ""SQL_CUSTOM_ONINIT"" value = ""NULL"" /> -->
                       
                               < !--< add key = ""SQL_CUSTOM_ONIMPORTMODEL"" value = ""NULL"" /> -->
                           
                                   < !--< add key = ""SQL_CUSTOM_ONEXIT"" value = ""NULL"" /> -->
                               
                                       < !--< add key = ""SQL_CUSTOM_ONRELOADORDERS"" value = ""NULL"" /> -->
                                   
                                           < !--< add key = ""SQL_CUSTOM_ONSYNC_BEFORE"" value = ""NULL"" /> -->
                                       
                                               < !--< add key = ""SQL_CUSTOM_ONSYNC_AFTER"" value = ""NULL"" /> -->
                                           
                                                   < !--< add key = ""SQL_CUSTOM_ONPREPAREDATA"" value = ""NULL"" /> -->
                                               

                                                   </ database_ganttplan >
                                               
                                                   < gui >
                                               
                                                       < !--Do you really want to? -->
                                               
                                                       < !-- < add key = ""DO_NOT_ASK_DELETE"" /> -->
                                                 
                                                         < !-- < add key = ""DO_NOT_ASK_SAVE"" /> -->
                                                   

                                                           < !--GUI Elemente deaktivieren -->
                                                   
                                                           < !--< add key = ""MAINFRAME_HIDE_TREE"" /> -->
                                                     
                                                             < !--< add key = ""MAINFRAME_HIDE_LIST"" /> -->
                                                       

                                                               < !--show time penalty controls in tabpage for worker(group) costs-- >
                                                       
                                                               < add key = ""SHOW_TIME_PENALTY_CONTROLS"" value = ""false"" />
                                                       

                                                               < !--hide context menu entries to add or delete BOM items in BOMs used by productionorders-- >
                                                       
                                                               < add key = ""DISALLOW_EDIT_USED_BOM_STRUCT"" value = ""false"" />
                                                       

                                                               < !--hide context menu entries to add or delete operations and activities in routings used by productionorders-- >
                                                       
                                                               < add key = ""DISALLOW_EDIT_USED_ROUTING_STRUCT"" value = ""false"" />
                                                       

                                                               < !--views deaktivieren : key = HIDE_VIEW_ * **-->
                                                       
                                                               < !--zb: < add key = ""VIEW_HIDE_OBJECT_material"" /> -->
                                                       

                                                               < !--fussliste deaktivieren : key = HIDE_LIST_ * **-->
                                                       
                                                               < add key = ""LIST_HIDE_OBJECT_unit"" />
                                                       
                                                               < add key = ""LIST_HIDE_OBJECT_optimizationgroup"" />
                                                       
                                                               < add key = ""LIST_HIDE_OBJECT_setupmatrix"" />
                                                       
                                                               < add key = ""LIST_HIDE_OBJECT_priority"" />
                                                       
                                                               < add key = ""LIST_HIDE_OBJECT_planningparameter"" />
                                                       
                                                               < add key = ""LIST_HIDE_OBJECT_user"" />
                                                       

                                                               < !--custom splashscreen(* ->bmp, png, jpg, gif)-- >
                                                       
                                                               < !--add key = ""SPLASHSCREEN_FILE"" value = ""MyLogo.*"" / -->
                                                       
                                                               < !--custom startscreen(* ->bmp, png, jpg, gif)-- >
                                                       
                                                               < !--add key = ""STARTSCREEN_FILE"" value = ""MyLogo.*"" / -->
                                                       
                                                               < !--custom logo(* ->bmp, png, jpg, gif) and url for toolbar-- >
                                                     
                                                             < !--add key = ""LOGO_FILE"" value = ""MyLogo.*"" / -->
                                                     
                                                             < !--add key = ""LOGO_URL"" value = ""http://www.mydomain.com"" / -->
                                                     

                                                             < !--Standardzeiteinheit in bestimmten Views festlegen : z.B.key = ""TIMEUNIT_routing"" value = ""101""-- >
                                                     
                                                             < !--Key untersützt aktuell routing, productionorder & bom-- >
                                                     
                                                             < !--Für value sind folgende Werte möglich:

                                                                 MINUTEN = 101
                                                     
                                                                 STUNDEN = 102
                                                     
                                                                 TAGE = 103
                                                             -- >
                                                     
                                                             < !-- < add key = ""TIMEUNIT_routing"" value = ""101"" /> -->
                                                     
                                                             < !-- < add key = ""TIMEUNIT_productionorder"" value = ""102"" /> -->
                                                     

                                                             < !--Spezielle Benennungen für benutzte Datenquellen in GANTTPLAN - Oberfläche
                                                     
                                                             Beispiel: value = ""ERP-System""-- >
                                                     
                                                             < add key = ""DATASOURCE_NAME_DBConfig"" value = """" />
                                                     
                                                             < add key = ""DATASOURCE_NAME_DBGP"" value = """" />
                                                     
                                                             < add key = ""DATASOURCE_NAME_SQLite"" value = """" />
                                                     
                                                             < add key = ""DATASOURCE_NAME_GPModel"" value = """" />
                                                     
                                                             < add key = ""DATASOURCE_NAME_Const"" value = """" />
                                                     
                                                             < add key = ""DATASOURCE_NAME_NULL"" value = """" />
                                                     
                                                             < add key = ""DATASOURCE_NAME_XMLConfig"" value = """" />
                                                     
                                                             < add key = ""DATASOURCE_NAME_Cache"" value = """" />
                                                     
                                                             < add key = ""DATASOURCE_NAME_Dll"" value = """" />
                                                     

                                                             < !--Regex - Expression for automatic URL detection in grid cells and tooltips-- >
                                                       
                                                               < add key = ""URL_DETECTION_REGEX_EXPRESSION"" value = ""(?:https?://|ftp://|file://|www.)[^\s].[^\s]+"" />
                                                       

                                                               < !--CSS, welches für die Darstellung von Leitstandstooltips verwendet wird.
                                                               Nomenklatur = .gantttooltip_ * PROPERTY_ID * { CSS}
                        Als CSS ist alles erlaubt, was ein<span></ span > in HTML 4 enthalten kann.

                    Im Beispiel wird der Hintergrund aller Eigenschaftsfelder der PropertyId workcenter_id die Hintergrundfarbe auf Rot gesetzt.
            
                    < add key = ""TOOLTIP_CSS"" value = "".gantttooltip_workcenter_id {bgcolor: #FF1414; }"" /> -->
   

           < !--Size of tooltip shadow-- >
   
           < !--add key = ""TOOLTIP_SHADOW_SIZE"" value = ""0"" / -->
      

              < !--The left, right and top, bottom margins of the tooltip's text from the tooltip's edges -->
      
              < !--add key = ""TOOLTIP_MARGIN_SIZE"" value = ""10"" / -->
         

                 < !--Hide GP logo in toolbar-- >
         
                 < add key = ""TOOLBAR_HIDE_GPLOGO"" value = ""false"" />
            

                    < !--Use this for dark toolbar background with white(bright) icons-- >
            
                    < add key = ""TOOLBAR_BRIGHT_ICONS"" value = ""false"" />
            

                    < !--show background image in startview-- >
            
                    < add key = ""STARTVIEW_SHOW_BACKGROUNDIMAGE"" value = ""true"" />
            

                </ gui >
            </ configuration >

                        "},
                new Config { PropertyName = "GanttPlan.config", Value = @"﻿﻿﻿﻿﻿﻿﻿<?xml version=""1.0"" encoding=""UTF-8""?>
<configuration>
<version/>

    <database_ganttplan>

        <add key=""CONNECTION_STRING"" value=""DRIVER={SQL Server Native Client 11.0};SERVER=(localdb)\MSSQLLocalDB;DATABASE=GanttPlanTisch;Trusted_connection=Yes;UID=;PWD=""/>

    </database_ganttplan>

    <database_extern>

        <add key=""CONNECTION_STRING"" value=""DRIVER={SQL Server Native Client 11.0};SERVER=(localdb)\MSSQLLocalDB;DATABASE=GanttPlan;Trusted_connection=yes;UID=sa;PWD=pwd;""/>

    </database_extern>

    <application>

        <add key=""PATH_LICENCEFILE"" value=""C:\ProgramData\DUALIS GmbH IT Solution""/>

        <add key=""LANGUAGE"" value=""DE""/>

    </application>
</configuration>

"
                },
                new Config { PropertyName = "colors.default.config", Value = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!-- Copyright © 2011-2018 DUALIS GmbH IT Solution - All Rights Reserved -->
<configuration>

    <version/>

    <gui>

        <!-- Beschreibung der wincolors: https://msdn.microsoft.com/en-us/library/windows/desktop/ms724371(v=vs.85).aspx -->


        < !--Thema - Farben-- >

        < !--Hintergrundfarbe von Edit-Controls bei gültigem Datensatz(grün)-- >

        < add key = ""GPCOLOR_101"" r = ""214"" g = ""240"" b = ""193"" />
       
               < !--Hintergrundfarbe von Edit-Controls bei ungültigem Datensatz(rot)-- >
       
               < add key = ""GPCOLOR_102"" r = ""250"" g = ""205"" b = ""184"" />
              
                      < !--Hintergrundfarbe der Tooltips -->
              
                      < add key = ""GPCOLOR_104"" r = ""252"" g = ""252"" b = ""252"" />
                     
                             < !--Rahmenfarbe der Tooltips -->
                     
                             < add key = ""GPCOLOR_105"" r = ""212"" g = ""212"" b = ""212"" />
                            
                                    < !--Hintergrundfarbe der Separatoren(dünne Linien) : Farbverlauf Verlauf von GPCOLOR_106 nach GPCOLOR_107 -->
                           
                                   < add key = ""GPCOLOR_106"" r = ""240"" g = ""130"" b = ""0"" />
                                  
                                          < add key = ""GPCOLOR_107"" r = ""240"" g = ""130"" b = ""0"" />
                                         
                                                 < !--Button für Szenariomodus starten / beenden-- >
                                         
                                                 < add key = ""GPCOLOR_108"" r = ""104"" g = ""176"" b = ""34"" />
                                                

                                                        < !--Hintergrundfarbe des Grids bei Mengenreduzierung eines Prognosebedarfs(Materialbeziehung) -->
                                               
                                                       < add key = ""GPCOLOR_109"" r = ""203"" g = ""200"" b = ""200"" />
                                                      

                                                              < !--kritisch-- >
                                                      
                                                              < add key = ""GPCOLOR_111"" r = ""240"" g = ""130"" b = ""0"" />
                                                             
                                                                     < !--verspätet-- >
                                                             
                                                                     < add key = ""GPCOLOR_112"" r = ""255"" g = ""0"" b = ""0"" />
                                                                    
                                                                            < !--nichtgeplant-- >
                                                                    
                                                                            < add key = ""GPCOLOR_113"" r = ""178"" g = ""0"" b = ""255"" />
                                                                           

                                                                                   < !--Farbverlauf Startscreen von GPCOLOR_114->nach GPCOLOR_115-- >
                                                                         
                                                                                 < add key = ""GPCOLOR_114"" r = ""252"" g = ""252"" b = ""252"" />
                                                                                
                                                                                        < add key = ""GPCOLOR_115"" r = ""252"" g = ""252"" b = ""252"" />
                                                                                       
                                                                                               < !--Schriftfarbe im Startscreen -->
                                                                                       
                                                                                               < add key = ""GPCOLOR_116"" r = ""20"" g = ""20"" b = ""20"" />
                                                                                              

                                                                                                      < !--gestartet-- >
                                                                                              
                                                                                                      < add key = ""GPCOLOR_117"" r = ""100"" g = ""255"" b = ""100"" />
                                                                                                     
                                                                                                             < !--beendet-- >
                                                                                                     
                                                                                                             < add key = ""GPCOLOR_118"" r = ""100"" g = ""100"" b = ""100"" />
                                                                                                            

                                                                                                                    < !--Farbe der dünnen horizontalen Trennlinie zwischen Objekttoolbar und View -->
                                                                                                            
                                                                                                                    < add key = ""GPCOLOR_120"" r = ""212"" g = ""212"" b = ""212"" />
                                                                                                                   
                                                                                                                           < !--Hintergrundfarbe des Menüs -->
                                                                                                                   
                                                                                                                           < add key = ""GPCOLOR_121"" r = ""246"" g = ""246"" b = ""246"" />
                                                                                                                          
                                                                                                                                  < !--Hintergrundfarbe markierter Einträge im Menü(Hover) -->
                                                                                                                         
                                                                                                                                 < add key = ""GPCOLOR_122"" r = ""214"" g = ""210"" b = ""210"" />
                                                                                                                                
                                                                                                                                        < !--Textfarbe des Menüs -->
                                                                                                                                
                                                                                                                                        < add key = ""GPCOLOR_123"" r = ""0"" g = ""0"" b = ""0"" />
                                                                                                                                       
                                                                                                                                               < !--Textfarbe des Menüs bei Selektierung -->
                                                                                                                                       
                                                                                                                                               < add key = ""GPCOLOR_124"" r = ""0"" g = ""0"" b = ""0"" />
                                                                                                                                              
                                                                                                                                                      < !--Hintergrundfarbe des selektierten Items in Kontextmenüs-- >
                                                                                                                                              
                                                                                                                                                      < add key = ""GPCOLOR_125"" r = ""214"" g = ""210"" b = ""210"" />
                                                                                                                                                     
                                                                                                                                                             < !--Textfarbe der Items in Kontextmenüs-- >
                                                                                                                                                     
                                                                                                                                                             < add key = ""GPCOLOR_126"" r = ""70"" g = ""70"" b = ""70"" />
                                                                                                                                                            
                                                                                                                                                                    < !--Textfarbe des selektierten Items in Kontextmenüs-- >
                                                                                                                                                            
                                                                                                                                                                    < add key = ""GPCOLOR_127"" r = ""70"" g = ""70"" b = ""70"" />
                                                                                                                                                                   

                                                                                                                                                                           < !--Rahmenfarbe der Panels -->
                                                                                                                                                                   
                                                                                                                                                                           < add key = ""GPCOLOR_130"" r = ""212"" g = ""212"" b = ""212"" />
                                                                                                                                                                          
                                                                                                                                                                                  < !--Hintergrundfarbe der Panel-Titelleiste(aktiv)-- >
                                                                                                                                                                          
                                                                                                                                                                                  < add key = ""GPCOLOR_131"" r = ""248"" g = ""248"" b = ""248"" />
                                                                                                                                                                                 
                                                                                                                                                                                         < !--Textfarbe der Panel-Titelleiste(aktiv)-- >
                                                                                                                                                                                 
                                                                                                                                                                                         < add key = ""GPCOLOR_132"" r = ""20"" g = ""20"" b = ""20"" />
                                                                                                                                                                                        
                                                                                                                                                                                                < !--Hintergrundfarbe der Panel-Titelleiste(inaktiv)-- >
                                                                                                                                                                                        
                                                                                                                                                                                                < add key = ""GPCOLOR_133"" r = ""248"" g = ""248"" b = ""248"" />
                                                                                                                                                                                               
                                                                                                                                                                                                       < !--Textfarbe der Panel-Titelleiste(inaktiv)-- >
                                                                                                                                                                                               
                                                                                                                                                                                                       < add key = ""GPCOLOR_134"" r = ""20"" g = ""20"" b = ""20"" />
                                                                                                                                                                                                      

                                                                                                                                                                                                              < !--Rahmenfarbe von Buttons -->
                                                                                                                                                                                                      
                                                                                                                                                                                                              < add key = ""GPCOLOR_140"" r = ""212"" g = ""212"" b = ""212"" />
                                                                                                                                                                                                             
                                                                                                                                                                                                                     < !--Hintergrundfarbe markierter Buttons(Hover) -->
                                                                                                                                                                                                            
                                                                                                                                                                                                                    < add key = ""GPCOLOR_141"" r = ""214"" g = ""210"" b = ""210"" />
                                                                                                                                                                                                                   
                                                                                                                                                                                                                           < !--Hintergrundfarbe gedrückter Buttons(Click) -->
                                                                                                                                                                                                                  
                                                                                                                                                                                                                          < add key = ""GPCOLOR_142"" r = ""210"" g = ""210"" b = ""210"" />
                                                                                                                                                                                                                         
                                                                                                                                                                                                                                 < !--Hintergrundfarbe aktivierter Buttons(Checked) -->
                                                                                                                                                                                                                        
                                                                                                                                                                                                                                < add key = ""GPCOLOR_143"" r = ""255"" g = ""255"" b = ""255"" />
                                                                                                                                                                                                                               
                                                                                                                                                                                                                                       < !--Rahmenfarbe aktivierter Buttons(Checked) (COLOR_HIGHLIGHTTEXT)-- >
                                                                                                                                                                                                                              
                                                                                                                                                                                                                                      < add key = ""GPCOLOR_144"" wincolor = ""13"" />
                                                                                                                                                                                                                                 

                                                                                                                                                                                                                                         < !--Hintergrundfarbe markierter Elemete in TreeCtrl(COLOR_HIGHLIGHT)-- >
                                                                                                                                                                                                                                 
                                                                                                                                                                                                                                         < add key = ""GPCOLOR_151"" wincolor = ""13"" />
                                                                                                                                                                                                                                    
                                                                                                                                                                                                                                            < !--Textfarbe markierter Elemete in TreeCtrl(COLOR_HIGHLIGHTTEXT)-- >
                                                                                                                                                                                                                                    
                                                                                                                                                                                                                                            < add key = ""GPCOLOR_152"" wincolor = ""14"" />
                                                                                                                                                                                                                                       
                                                                                                                                                                                                                                               < !--Hintergrundfarbe markierter Elemete in TreeCtrl, wenn das Control keinen Fokus besitzt -->
                                                                                                                                                                                                                                       
                                                                                                                                                                                                                                               < add key = ""GPCOLOR_154"" r = ""214"" g = ""210"" b = ""210"" />
                                                                                                                                                                                                                                              
                                                                                                                                                                                                                                                      < !--Textfarbe markierter Elemete in TreeCtrl, wenn das Control keinen Fokus besitzt -->
                                                                                                                                                                                                                                              
                                                                                                                                                                                                                                                      < add key = ""GPCOLOR_155""  r = ""0"" g = ""0"" b = ""0"" />
                                                                                                                                                                                                                                                     
                                                                                                                                                                                                                                                             < !--Rahmenfarbe der TreeCtrls -->
                                                                                                                                                                                                                                                     
                                                                                                                                                                                                                                                             < add key = ""GPCOLOR_164"" r = ""212"" g = ""212"" b = ""212"" />
                                                                                                                                                                                                                                                            

                                                                                                                                                                                                                                                                    < !--Hintergrundfarbe des Navigationsbaums -->
                                                                                                                                                                                                                                                            
                                                                                                                                                                                                                                                                    < add key = ""GPCOLOR_150"" r = ""248"" g = ""248"" b = ""248"" />
                                                                                                                                                                                                                                                                   
                                                                                                                                                                                                                                                                           < !--Textfarbe nicht markierter Elemete im Navigationsbaum-- >
                                                                                                                                                                                                                                                                   
                                                                                                                                                                                                                                                                           < add key = ""GPCOLOR_153"" r = ""0"" g = ""0"" b = ""0"" />
                                                                                                                                                                                                                                                                          
                                                                                                                                                                                                                                                                                  < !--Hintergrundfarbe markierter Elemete im Navigationsbaum(COLOR_HIGHLIGHT) -->
                                                                                                                                                                                                                                                                         
                                                                                                                                                                                                                                                                                 < add key = ""GPCOLOR_156"" wincolor = ""13"" />
                                                                                                                                                                                                                                                                            
                                                                                                                                                                                                                                                                                    < !--Textfarbe markierter Elemete im Navigationsbaum(COLOR_HIGHLIGHTTEXT) -->
                                                                                                                                                                                                                                                                           
                                                                                                                                                                                                                                                                                   < add key = ""GPCOLOR_157"" wincolor = ""14"" />
                                                                                                                                                                                                                                                                              
                                                                                                                                                                                                                                                                                      < !--Hintergrundfarbe markierter Elemete im Navigationsbaum, wenn das Control keinen Fokus besitzt-- >
                                                                                                                                                                                                                                                                              
                                                                                                                                                                                                                                                                                      < add key = ""GPCOLOR_158"" r = ""214"" g = ""210"" b = ""210"" />
                                                                                                                                                                                                                                                                                     
                                                                                                                                                                                                                                                                                             < !--Textfarbe markierter Elemete im Navigationsbaum, wenn das Control keinen Fokus besitzt-- >
                                                                                                                                                                                                                                                                                     
                                                                                                                                                                                                                                                                                             < add key = ""GPCOLOR_159""  r = ""0"" g = ""0"" b = ""0"" />
                                                                                                                                                                                                                                                                                            

                                                                                                                                                                                                                                                                                                    < !--Hintergrundfarbe der Statusleiste -->
                                                                                                                                                                                                                                                                                            
                                                                                                                                                                                                                                                                                                    < add key = ""GPCOLOR_168"" r = ""248"" g = ""248"" b = ""248"" />
                                                                                                                                                                                                                                                                                                   
                                                                                                                                                                                                                                                                                                           < !--Textfarbe der Statusleiste -->
                                                                                                                                                                                                                                                                                                   
                                                                                                                                                                                                                                                                                                           < add key = ""GPCOLOR_169"" r = ""0"" g = ""0"" b = ""0"" />
                                                                                                                                                                                                                                                                                                          
                                                                                                                                                                                                                                                                                                                  < !--Hintergrundfarbe der Statusleiste, wenn Anwendung beschäftigt(busy) -->
                                                                                                                                                                                                                                                                                                         
                                                                                                                                                                                                                                                                                                                 < add key = ""GPCOLOR_160"" r = ""248"" g = ""248"" b = ""248"" />
                                                                                                                                                                                                                                                                                                                
                                                                                                                                                                                                                                                                                                                        < !--Textfarbe der Statusleiste, wenn Anwendung beschäftigt(busy) -->
                                                                                                                                                                                                                                                                                                               
                                                                                                                                                                                                                                                                                                                       < add key = ""GPCOLOR_161"" r = ""0"" g = ""0"" b = ""0"" />
                                                                                                                                                                                                                                                                                                                      
                                                                                                                                                                                                                                                                                                                              < !--Hintergrundfarbe der Statusleiste, wenn im Planungsmodus -->
                                                                                                                                                                                                                                                                                                                      
                                                                                                                                                                                                                                                                                                                              < add key = ""GPCOLOR_162"" r = ""240"" g = ""130"" b = ""0"" />
                                                                                                                                                                                                                                                                                                                             
                                                                                                                                                                                                                                                                                                                                     < !--Textfarbe der Statusleiste, wenn im Planungsmodus -->
                                                                                                                                                                                                                                                                                                                             
                                                                                                                                                                                                                                                                                                                                     < add key = ""GPCOLOR_163"" r = ""255"" g = ""255"" b = ""255"" />
                                                                                                                                                                                                                                                                                                                                    
                                                                                                                                                                                                                                                                                                                                            < !--Farbe der Progressbar in der Statusleiste -->
                                                                                                                                                                                                                                                                                                                                    
                                                                                                                                                                                                                                                                                                                                            < add key = ""GPCOLOR_175"" r = ""240"" g = ""130"" b = ""0"" />
                                                                                                                                                                                                                                                                                                                                           
                                                                                                                                                                                                                                                                                                                                                   < !--Hintergrundfarbe der Haupttoolbar -->
                                                                                                                                                                                                                                                                                                                                           
                                                                                                                                                                                                                                                                                                                                                   < add key = ""GPCOLOR_165"" r = ""246"" g = ""246"" b = ""246"" />
                                                                                                                                                                                                                                                                                                                                                  
                                                                                                                                                                                                                                                                                                                                                          < !--Hintergrundfarbe der Leitstandstoolbar -->
                                                                                                                                                                                                                                                                                                                                                  
                                                                                                                                                                                                                                                                                                                                                          < add key = ""GPCOLOR_166""  r = ""250"" g = ""250"" b = ""250"" />
                                                                                                                                                                                                                                                                                                                                                         
                                                                                                                                                                                                                                                                                                                                                                 < !--Hintergrundfarbe der Reportoolbar -->
                                                                                                                                                                                                                                                                                                                                                         
                                                                                                                                                                                                                                                                                                                                                                 < add key = ""GPCOLOR_167"" r = ""252"" g = ""252"" b = ""252"" />
                                                                                                                                                                                                                                                                                                                                                                

                                                                                                                                                                                                                                                                                                                                                                        < !--Hintergrundfarbe der Views -->
                                                                                                                                                                                                                                                                                                                                                                
                                                                                                                                                                                                                                                                                                                                                                        < add key = ""GPCOLOR_170""  r = ""252"" g = ""252"" b = ""252"" />
                                                                                                                                                                                                                                                                                                                                                                       

                                                                                                                                                                                                                                                                                                                                                                               < !--Hintergrundfarbe der Tab-Controls-- >
                                                                                                                                                                                                                                                                                                                                                                       
                                                                                                                                                                                                                                                                                                                                                                               < add key = ""GPCOLOR_180""  r = ""255"" g = ""255"" b = ""255"" />
                                                                                                                                                                                                                                                                                                                                                                              
                                                                                                                                                                                                                                                                                                                                                                                      < !--Hintergrundfarbe der nicht selektierten Tabs(Normal) -->
                                                                                                                                                                                                                                                                                                                                                                             
                                                                                                                                                                                                                                                                                                                                                                                     < add key = ""GPCOLOR_181"" r = ""252"" g = ""252"" b = ""252"" />
                                                                                                                                                                                                                                                                                                                                                                                    
                                                                                                                                                                                                                                                                                                                                                                                            < !--Textfarbe der nicht selektierten Tabs(Normal) -->
                                                                                                                                                                                                                                                                                                                                                                                   
                                                                                                                                                                                                                                                                                                                                                                                           < add key = ""GPCOLOR_182""  r = ""0"" g = ""0"" b = ""0"" />
                                                                                                                                                                                                                                                                                                                                                                                          
                                                                                                                                                                                                                                                                                                                                                                                                  < !--Hintergrundfarbe der selektierten Tabs(Selected)-- >
                                                                                                                                                                                                                                                                                                                                                                                          
                                                                                                                                                                                                                                                                                                                                                                                                  < add key = ""GPCOLOR_183""  r = ""255"" g = ""255"" b = ""255"" />
                                                                                                                                                                                                                                                                                                                                                                                                 
                                                                                                                                                                                                                                                                                                                                                                                                         < !--Textfarbe der selektierten Tabs(Selected)-- >
                                                                                                                                                                                                                                                                                                                                                                                                 
                                                                                                                                                                                                                                                                                                                                                                                                         < add key = ""GPCOLOR_184""  r = ""0"" g = ""0"" b = ""0"" />
                                                                                                                                                                                                                                                                                                                                                                                                        
                                                                                                                                                                                                                                                                                                                                                                                                                < !--Hintergrundfarbe der Tabs, über denen sich die Maus befindet(Hover)-- >
                                                                                                                                                                                                                                                                                                                                                                                                        
                                                                                                                                                                                                                                                                                                                                                                                                                < add key = ""GPCOLOR_185"" r = ""214"" g = ""210"" b = ""210"" />
                                                                                                                                                                                                                                                                                                                                                                                                               
                                                                                                                                                                                                                                                                                                                                                                                                                       < !--Textfarbe der Tabs, über denen sich die Maus befindet(Hover)-- >
                                                                                                                                                                                                                                                                                                                                                                                                               
                                                                                                                                                                                                                                                                                                                                                                                                                       < add key = ""GPCOLOR_186""  r = ""0"" g = ""0"" b = ""0"" />
                                                                                                                                                                                                                                                                                                                                                                                                                      
                                                                                                                                                                                                                                                                                                                                                                                                                              < !--Rahmenfarbe der Tab-Controls-- >
                                                                                                                                                                                                                                                                                                                                                                                                                      
                                                                                                                                                                                                                                                                                                                                                                                                                              < add key = ""GPCOLOR_187"" r = ""212"" g = ""212"" b = ""212"" />
                                                                                                                                                                                                                                                                                                                                                                                                                             
                                                                                                                                                                                                                                                                                                                                                                                                                                     < !--Hintergrundfarbe der Tab-Teilüberschriften-- >
                                                                                                                                                                                                                                                                                                                                                                                                                             
                                                                                                                                                                                                                                                                                                                                                                                                                                     < add key = ""GPCOLOR_188"" wincolor = ""4"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                
                                                                                                                                                                                                                                                                                                                                                                                                                                        < !--Textfarbe der Tab-Teilüberschriften-- >
                                                                                                                                                                                                                                                                                                                                                                                                                                
                                                                                                                                                                                                                                                                                                                                                                                                                                        < add key = ""GPCOLOR_189"" r = ""119"" g = ""119"" b = ""119"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                       

                                                                                                                                                                                                                                                                                                                                                                                                                                               < !--Hintergrundfarbe der Dialoge -->
                                                                                                                                                                                                                                                                                                                                                                                                                                       
                                                                                                                                                                                                                                                                                                                                                                                                                                               < add key = ""GPCOLOR_190""  r = ""252"" g = ""252"" b = ""252"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                              

                                                                                                                                                                                                                                                                                                                                                                                                                                                      < !--Hintergrund der fixierten Grid - Kopfzellen-- >
                                                                                                                                                                                                                                                                                                                                                                                                                                              
                                                                                                                                                                                                                                                                                                                                                                                                                                                      < add key = ""GPCOLOR_202"" r = ""230"" g = ""230"" b = ""230"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                     
                                                                                                                                                                                                                                                                                                                                                                                                                                                             < !--Textfarbe der fixierten Grid - Kopfzellen-- >
                                                                                                                                                                                                                                                                                                                                                                                                                                                     
                                                                                                                                                                                                                                                                                                                                                                                                                                                             < add key = ""GPCOLOR_209"" r = ""0"" g = ""0"" b = ""0"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                            
                                                                                                                                                                                                                                                                                                                                                                                                                                                                    < !--Hintergrund der Grid-Zellen, wenn editierbar -->
                                                                                                                                                                                                                                                                                                                                                                                                                                                            
                                                                                                                                                                                                                                                                                                                                                                                                                                                                    < add key = ""GPCOLOR_203"" r = ""255"" g = ""255"" b = ""255"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
                                                                                                                                                                                                                                                                                                                                                                                                                                                                           < !--Hintergrund der Grid-Zellen, wenn nicht editierbar-- >
                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
                                                                                                                                                                                                                                                                                                                                                                                                                                                                           < add key = ""GPCOLOR_204"" r = ""245"" g = ""245"" b = ""245"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                          
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  < !--Hintergrund der Grid-Zellen, wenn selektiert(COLOR_HIGHLIGHT) -->
                                                                                                                                                                                                                                                                                                                                                                                                                                                                         
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 < add key = ""GPCOLOR_205"" wincolor = ""13"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                            
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    < !--Rahmen der Grid-Zellen, wenn selektiert(COLOR_HIGHLIGHT) -->
                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   < add key = ""GPCOLOR_210"" wincolor = ""14"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                              
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      < !--Textfarbe von Grid-Zellen, wenn selektiert(COLOR_HIGHLIGHTTEXT) -->
                                                                                                                                                                                                                                                                                                                                                                                                                                                                             
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     < add key = ""GPCOLOR_208"" wincolor = ""14"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        < !--Textfarbe von Grid-Zellen, mit Link -->
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        < add key = ""GPCOLOR_206"" r = ""0"" g = ""110"" b = ""180"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               < !--Hintergrund der Grid-Zellen, wenn Option Zeilenmarkierung im Report aktiviert -->
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               < add key = ""GPCOLOR_207"" r = ""215"" g = ""215"" b = ""215"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      < !--Hintergrund der Grid-Zellen, wenn selektiert, das Grid aber keinen Fokus besitzt-- >
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      < add key = ""GPCOLOR_211"" r = ""214"" g = ""210"" b = ""210"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             < !--Rahmen der Grid-Zellen, wenn selektiert, das Grid aber keinen Fokus besitzt-- >
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             < add key = ""GPCOLOR_213"" r = ""214"" g = ""210"" b = ""210"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    < !--Textfarbe von Grid-Zellen, wenn selektiert, das Grid aber keinen Fokus besitzt-- >
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    < add key = ""GPCOLOR_212""  r = ""0"" g = ""0"" b = ""0"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           < !--Farbe der Gridlinien -->
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           < add key = ""GPCOLOR_214"" r = ""192"" g = ""192"" b = ""192"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          

                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  < !--Farbeinstellungen im Leitstand -->
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  < !--Hintergrundfarbe für Arbeitszeiten(weiß) -->
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 < add key = ""GPCOLOR_300"" r = ""255"" g = ""255"" b = ""255"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        < !--Hintergrundfarbe für Pausezeiten -->
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        < add key = ""GPCOLOR_301"" r = ""230"" g = ""230"" b = ""230"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               < !--Farbe des Grids im Leitstand -->
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               < add key = ""GPCOLOR_302"" r = ""210"" g = ""210"" b = ""210"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      < !--Farbe des Subrasters vom Kalender -->
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      < add key = ""GPCOLOR_303"" r = ""140"" g = ""138"" b = ""138"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             < !--normale Textfarbe für Beschriftungen(z.B.angezeigte Ressourcen, Kalender, ...)-- >
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             < add key = ""GPCOLOR_304"" r = ""0"" g = ""0"" b = ""0"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    < !--Textfarbe für nicht ausgewählte angezeigte Ressourcen(inaktiv)-- >
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    < add key = ""GPCOLOR_305"" r = ""211"" g = ""211"" b = ""211"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           < !--Balkenmarkierung für freigegebene Fertigungsaufträge-- >
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           < add key = ""GPCOLOR_310"" r = ""255"" g = ""255"" b = ""255"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  < !--Balkenmarkierung für fixierte Vorgänge-- >
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  < add key = ""GPCOLOR_311"" r = ""14"" g = ""99"" b = ""156"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 

                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         < !--Farbeinstellungen für den Splashscreen-- >
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         < !--Hintergrundfarbe des Splashscreen -->
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         < add key = ""GPCOLOR_400"" r = ""240"" g = ""130"" b = ""0"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                < !--Rahmenfarbe des Splashscreen -->
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                < add key = ""GPCOLOR_401"" r = ""240"" g = ""130"" b = ""0"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       < !--Textfarbe des Splashscreen -->
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       < add key = ""GPCOLOR_402"" r = ""255"" g = ""255"" b = ""255"" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          </ gui >
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      </ configuration >
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      "
                },
                new Config { PropertyName = "icons.default.config", Value = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!-- Copyright © 2015-2018 DUALIS GmbH IT Solution - All Rights Reserved -->
<configuration>

    <version/>

    <gui>


        <!-- Angabe, ob die Icongröße entsprechend der Windows-Skalierungsstufe angepasst werden soll

        true: 	Icongröße wird angepasst(Default)

        false: 	Icongröße wird nicht angepasst -->

        < add key = ""SCALE_ICONS"" value = ""true"" />
   

           < !---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

           Erklärung:	Der Großteil der Icons in GANTTPLAN hat die Standardgröße 16x16.Falls die Größe eines Icons vom Standard abweicht, ist dessen Standardgröße

                    im Kommentar hinter dem Config - Eintrag hinterlegt.Die Standardgröße gilt für eine Skalierungsstufe von 100 %.Es können auch größere Bilder verwendet werden, die

                    Bildrelation sollte aber erhalten bleiben.Anstatt 16x16 kann also auch ein Icon mit 24x24 verwendet werden.Das Icon wird dann entsprechend skaliert.


        Hinweis: Kann ein Icon nicht erfolgreich geladen werden(Datei nicht gefunden, falsches Dateiformat etc.), dann wird stattdessen das Standardicon geladen und ein entsprechender
                Eintrag in die GANTTPLAN-Logdatei geschrieben.


        Folgende Pfadangaben können genutzt werden:

            Absolute Pfadangaben

                ""C:\Icons\Mustericon.ico""

            Relative Pfadangaben

                ""Mustericon.ico"" - Icon liegt im selben Verzeichnis wie GANTTPLAN.exe

                ""..\Mustericon.ico"" - Icon liegt im übergeordneten Ordner der GANTTPLAN.exe

                ""Unterordner/Mustericon.ico"" - Icon liegt in einem Unterverzeichnis der GANTTPLAN.exe

        Folgende Dateiformate können geladen werden:
		
			PNG(mit Transparenz)

            BMP(mit Transparenz - RGB(255, 0, 255) als transparente Farbe oder über Alpha - Kanal)

            ICO(mit Transparenz(32bit - Farbtiefe notwendig))

            TIF / TIFF(mit Transparenz)

            GIF(keine Transparenz, im Original transparenter Bereich wird weiß)

            JPG / JPEG(keine Transparenz)

        ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- >


        < add key = ""ICON_ARROW_LEFT"" value = """" />
   
           < add key = ""ICON_ARROW_RIGHT"" value = """" />
      
              < add key = ""ICON_ARROW_DOWN"" value = """" /> < !--size: 20 x 20-- >
            
                    < add key = ""ICON_ARROW_UP"" value = """" /> < !--size: 20 x 20-- >
                  
                          < add key = ""ICON_CONFIGURATION"" value = """" />
                     
                             < add key = ""ICON_ACTIVITYQUALIFICATION_BOTH"" value = """" />
                        
                                < add key = ""ICON_ACTIVITYQUALIFICATION_INDIRECT"" value = """" />
                           
                                   < add key = ""ICON_ACTIVITYQUALIFICATION_DIRECT"" value = """" />
                              
                                      < add key = ""ICON_ADD"" value = """" />
                                 
                                         < add key = ""ICON_ADD_ALL"" value = """" />
                                    
                                            < add key = ""ICON_VIEW_CLEAR"" value = """" />
                                       
                                               < add key = ""ICON_PRODUCTIONORDER"" value = """" />
                                          
                                                  < add key = ""ICON_WORKCENTER"" value = """" />       < add key = ""ICON_WORKER"" value = """" />
                                                   
                                                           < add key = ""ICON_PRT"" value = """" />
                                                      
                                                              < add key = ""ICON_SAVE"" value = """" />
                                                         
                                                                 < add key = ""ICON_PO_PLANABLE"" value = """" />
                                                            
                                                                    < add key = ""ICON_PO_NOT_PLANABLE"" value = """" />
                                                               
                                                                       < add key = ""ICON_COLORSCHEME"" value = """" />
                                                                  
                                                                          < add key = ""ICON_CALENDAR"" value = """" />
                                                                     
                                                                             < add key = ""ICON_CATEGORY_BACK"" value = """" />
                                                                        
                                                                                < add key = ""ICON_CATEGORY_FORWARD"" value = """" />
                                                                           
                                                                                   < add key = ""ICON_CHANGELOG"" value = """" />
                                                                              
                                                                                      < add key = ""ICON_CLOCK"" value = """" />        < add key = ""ICON_COLORPALLET"" value = """" />
                                                                                       
                                                                                               < add key = ""ICON_CONNECTION"" value = """" />
                                                                                          
                                                                                                  < add key = ""ICON_GANTTVIEW_PO"" value = """" /> < !--size: 23 x 16-- >
                                                                                                
                                                                                                        < add key = ""ICON_GANTTVIEW_RES"" value = """" /> < !--size: 23 x 16-- >
                                                                                                      
                                                                                                              < add key = ""ICON_GANTTVIEW_STD"" value = """" /> < !--size: 23 x 16-- >
                                                                                                            
                                                                                                                    < add key = ""ICON_EDIT"" value = """" />
                                                                                                               
                                                                                                                       < add key = ""ICON_EXCEL_EXPORT"" value = """" />
                                                                                                                  
                                                                                                                          < add key = ""ICON_FILTER_INACTIVE"" value = """" />
                                                                                                                     
                                                                                                                             < add key = ""ICON_FILTER_ACTIVE"" value = """" />
                                                                                                                        
                                                                                                                                < add key = ""ICON_FOLDER"" value = """" />
                                                                                                                           
                                                                                                                                   < add key = ""ICON_FORMULA"" value = """" />
                                                                                                                              
                                                                                                                                      < add key = ""ICON_FULLSCREEN"" value = """" />       < add key = ""ICON_GRID_TREE_COLLAPSED"" value = """" /> < !--size: 25 x 16-- >
                                                                                                                                          
                                                                                                                                                  < add key = ""ICON_GRID_TREE_EXPANDED"" value = """" /> < !--size: 25 x 16-- >
                                                                                                                                                
                                                                                                                                                        < add key = ""ICON_GRID_TREE_JUMP"" value = """" /> < !--size: 25 x 16-- >
                                                                                                                                                      
                                                                                                                                                              < add key = ""ICON_GRID_TREE_JUMP_COLLAPSED"" value = """" /> < !--size: 25 x 16-- >
                                                                                                                                                            
                                                                                                                                                                    < add key = ""ICON_GRID_TREE_JUMP_EXPANDED"" value = """" /> < !--size: 25 x 16-- >
                                                                                                                                                                  
                                                                                                                                                                          < add key = ""ICON_MANUAL_MODE"" value = """" />
                                                                                                                                                                     
                                                                                                                                                                             < add key = ""ICON_HELP"" value = """" />
                                                                                                                                                                        
                                                                                                                                                                                < add key = ""ICON_ACTUAL_TIME"" value = """" />
                                                                                                                                                                           
                                                                                                                                                                                   < add key = ""ICON_JUMPMARK"" value = """" />
                                                                                                                                                                              
                                                                                                                                                                                      < add key = ""ICON_LIVE_MODUS"" value = """" />
                                                                                                                                                                                 
                                                                                                                                                                                         < add key = ""ICON_RESSOURCESCHEDULE_NEXT_WEEK"" value = """" />
                                                                                                                                                                                    
                                                                                                                                                                                            < add key = ""ICON_RESSOURCESCHEDULE_PREV_WEEK"" value = """" />
                                                                                                                                                                                       
                                                                                                                                                                                               < add key = ""ICON_PERMISSION"" value = """" />
                                                                                                                                                                                          
                                                                                                                                                                                                  < add key = ""ICON_PRINT"" value = """" />
                                                                                                                                                                                             
                                                                                                                                                                                                     < add key = ""ICON_PROPERTY"" value = """" />
                                                                                                                                                                                                
                                                                                                                                                                                                        < add key = ""ICON_RELOAD_MODEL"" value = """" />
                                                                                                                                                                                                   
                                                                                                                                                                                                           < add key = ""ICON_REMOVE"" value = """" />
                                                                                                                                                                                                      
                                                                                                                                                                                                              < add key = ""ICON_RESET"" value = """" />
                                                                                                                                                                                                         
                                                                                                                                                                                                                 < add key = ""ICON_SELECT"" value = """" />
                                                                                                                                                                                                            
                                                                                                                                                                                                                    < add key = ""ICON_SHEET"" value = """" />
                                                                                                                                                                                                               
                                                                                                                                                                                                                       < add key = ""ICON_DATASET_SAVE"" value = """" />
                                                                                                                                                                                                                  
                                                                                                                                                                                                                          < add key = ""ICON_DATASET_PREV"" value = """" />
                                                                                                                                                                                                                     
                                                                                                                                                                                                                             < add key = ""ICON_DATASET_COPY"" value = """" />
                                                                                                                                                                                                                        
                                                                                                                                                                                                                                < add key = ""ICON_DATASET_DELETE"" value = """" />
                                                                                                                                                                                                                           
                                                                                                                                                                                                                                   < add key = ""ICON_DATASET_EDIT"" value = """" />
                                                                                                                                                                                                                              
                                                                                                                                                                                                                                      < add key = ""ICON_DATASET_NEW"" value = """" />
                                                                                                                                                                                                                                 
                                                                                                                                                                                                                                         < add key = ""ICON_DATASET_NEXT"" value = """" />
                                                                                                                                                                                                                                    
                                                                                                                                                                                                                                            < add key = ""ICON_DATASET_DISCARD"" value = """" />
                                                                                                                                                                                                                                       
                                                                                                                                                                                                                                               < add key = ""ICON_SEARCH"" value = """" />       < add key = ""ICON_SWAP"" value = """" />
                                                                                                                                                                                                                                                
                                                                                                                                                                                                                                                        < add key = ""ICON_SCENARIO_DISCARD"" value = """" /> < !--size: 48 x 48-- >
                                                                                                                                                                                                                                                      
                                                                                                                                                                                                                                                              < add key = ""ICON_SCENARIO_SAVE"" value = """" /> < !--size: 48 x 48-- >
                                                                                                                                                                                                                                                            
                                                                                                                                                                                                                                                                    < add key = ""ICON_SCENARIO_ACCEPT"" value = """" /> < !--size: 48 x 48-- >
                                                                                                                                                                                                                                                                  
                                                                                                                                                                                                                                                                          < add key = ""ICON_UPDATE_BDE"" value = """" />
                                                                                                                                                                                                                                                                     
                                                                                                                                                                                                                                                                             < add key = ""ICON_ZOOM_IN"" value = """" />
                                                                                                                                                                                                                                                                        
                                                                                                                                                                                                                                                                                < add key = ""ICON_ZOOM_OUT"" value = """" />
                                                                                                                                                                                                                                                                           
                                                                                                                                                                                                                                                                                   < add key = ""ICON_LOCKED"" value = """" />
                                                                                                                                                                                                                                                                              
                                                                                                                                                                                                                                                                                      < add key = ""ICON_UNLOCKED"" value = """" />
                                                                                                                                                                                                                                                                                 
                                                                                                                                                                                                                                                                                         < add key = ""ICON_CREATE_ORDERRELATED_ROUTING"" value = """" />
                                                                                                                                                                                                                                                                                    

                                                                                                                                                                                                                                                                                        </ gui >
                                                                                                                                                                                                                                                                                    </ configuration >
                                                                                                                                                                                                                                                                                    "},
                new Config { PropertyName = "interface-export.default.config", Value = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!-- Copyright © 2010-2018 DUALIS GmbH IT Solution - All Rights Reserved -->
<configuration>

    <version/>

    <export>

        <!-- Exportmodus (CSV,DIRECTSQL,BULK) -->

        <add key=""MODE"" value=""CSV""/>


        <!-- CSV,BULK: Path to write files, e.g. \\Server\public\ -->
		<!-- if empty, then the users working directory\Export\ is used, default: %userprofile%\documents\GANTTPLAN\Export\ -->
		<add key = ""PATH_GPTOPUBLICSHARE"" value=""""/>
		<!-- BULK: Path to read files, e.g C:\Shares\public\ -->
		<add key = ""PATH_DBTOPUBLICSHARE"" value=""""/>
		
		<!-- BULK,DIRECTSQL: welchen Connection string nutzen: DBGP, DBConfig -->
		<add key = ""CONNECTION"" value=""DBConfig""/>
		<!-- BULK,DIRECTSQL: custom sql command -->
		<add key = ""ONINIT"" value=""NULL""/>
		<!-- BULK,DIRECTSQL: custom sql command -->
		<add key = ""ONEXIT"" value=""NULL""/>
		<!-- BULK,DIRECTSQL,CSV: defines maximum string length per column(-1 == unlimited) -->
		<add key = ""MAXLENGTH"" value=""-1""/>
		<!-- DIRECTSQL: Enable automatic creation of exporttables if directsql export is configured, connection is dbgp and user has ""Alter Database"" permission -->
		<add key = ""CREATETABLES"" value=""false"" />
		<!-- DIRECTSQL: Dürfen mehrere Insert-Statements gesammelt werden, die dann gemeinsam in einem Befehl übergeben werden -->
		<add key = ""MULTIINSERT"" value=""true""/>
		<!-- DIRECTSQL: Spaltennamen werden damit umschlossen, um sich von reservierten Schlüsselwörtern zu unterscheiden(z.B. [count] oder ""count"" anstatt nur count) -->
		<!-- Entweder zwei Zeichen angeben(z.B. ""[]""), falls öffnender und schließender Identifier unterschiedlich, oder nur ein Zeichen angeben(z.B. ""%""), falls identisch -->
		<!-- Handelt es sich beim Identifier um Quotes '""', dann muss dies mit dem Schlüsselwort ""&quot;"" (ähnlich HTML-Entities) angegeben werden -->
		<add key = ""IDENTIFIER"" value=""[]""/>
		<!-- DIRECTSQL: enable transaction security in directsql-export(rollback if error occurs) -->
		<add key = ""TRANSACTION_SECURITY"" value=""true""/>
						
		<!-- BULK: Name der Exportdatenbank(nur angeben, falls abweichend von Datenbank aus Connection-String) -->
		<add key = ""DATABASE"" value=""""/>
		<!-- CSV,BULK: Dateierweiterung -->
		<add key = ""FILEEXT"" value=""txt""/>
		<!-- CSV,BULK: Trennzeichen -->
		<add key = ""DELIMITER"" value=""¿""/>
		<!-- CSV,BULK: write header information in first line -->
		<add key = ""HEADER"" value=""true""/>
		<!-- CSV: enclose strings with double quotes(like in EXCEL), e.g. ""My String"" -->
		<add key = ""QUOTES"" value=""false""/>
		<!-- CSV,BULK,DIRECTSQL: Dateien bzw.Tabellen werden vor dem Füllen gelöscht -->
		<add key = ""DELETE"" value= ""true"" />

        < !--BULK: stored procedure that handles bulk insert -->
		<add key = ""BULKINSERT_SP"" value= ""{? = CALL gpsp_export_bulkinsert('%s','%s','%s','%s','%s','%s',%s)}"" />

        < !--BULK: temporär erzeugte CSV-Dateien nach BULK Export löschen -->
		<add key = ""BULK_DELETE_FILES"" value= ""true"" />

        < !--BULK, DIRECTSQL, CSV: Configures SESSION Data which is written in export (for all object types). Possible Values: client_id|session_id|result_id|userhost -->
		<add key = ""EXPORT_SESSIONDATA"" value=""session_id|client_id|result_id"" />
		<!-- BULK,DIRECTSQL,CSV: renaming of sessiondata-entries in export, e.g ""ALIAS_SESSIONDATA_userhost"" value=""computer""
		<add key = ""ALIAS_SESSIONDATA_[sessiondata]"" value=""""/> -->
		
		<!-- Object-Export -->
		<!-- ENABLE_[objecttype]: enable/disable export(use* to export all objecttypes) -->
		<!-- TARGET_[objecttype]: set custom table- or filename, default if no value is given: then the value is automatically replaced by the key of the current objecttype -->
		<!-- CONFIG_[objecttype]: 'Pipe' seperated list of all properties to export: e.g.info1|info2|info3, default if no value is given: all properties -->
		
		<!-- example with objecttype material and only session_id from session data
        <add key= ""ENABLE_material"" />

        < add key= ""TARGET_material"" value= ""FileOrTablename"" />

        < add key= ""CONFIG_material"" value= ""info1|info2|info3"" />

        < add key= ""EXPORT_SESSIONDATA"" value= ""session_id"" /> -->


        < !--Report - Export-- >

        < !--ENABLE_REPORT_EXPORT_[report]: enable/disable report export (replace[report] with its report_id) -->
		<!-- TARGET_REPORT_EXPORT_[report]: configures the target table or file for the configured report -->
		<!-- REPORT_EXPORT_COLUMNS_[report]: 'Pipe' seperated list of all properties to export, default if no value is given: all properties -->
		
		<!-- example with default report salesorders(report_id = 7)
		<add key = ""ENABLE_REPORT_EXPORT_7"" />

        < add key=""TARGET_REPORT_EXPORT_7"" value=""FileOrTablename""/>
		<add key = ""REPORT_EXPORT_COLUMNS_7"" value=""salesorder.salesorder_id|salesorder.unit.unit_id|salesorder.material.name|salesorder.report_stock_withdrawal|salesorder.report_end_schedule""/> -->
		
		<!-- Report-Export alias names -->
		<!-- REPORT_EXPORT_ALIAS_[report]:[property]: use alias name for property in report -->
		
		<!-- example alias 'salesorder_number' for property 'salesorder.salesorder_id' of default report salesorders(report_id = 7)
		<!-- notice: if this alias is used you also have to use this alias in REPORT_EXPORT_COLUMNS
        <add key=""REPORT_EXPORT_ALIAS_7:salesorder.salesorder_id"" value=""salesorder_number""/> -->
	</export>	
</configuration>
"},
                new Config { PropertyName = "interface-import.default.config", Value = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!-- Copyright © 2010-2018 DUALIS GmbH IT Solution - All Rights Reserved -->
<configuration>

    <version/>

    <database_ganttplan>

        <!-- Standard ist der MSSQL-Provider, möglich ist jeder OLE DB provider -->

        <add key=""SYNCHRONIZATION_PROVIDER"" value=""SQLNCLI""/>

        <!-- Anzahl Sekunden, die seit der letzten Synchronisierung vergangen sein muss, damit bei Bedarf wieder automatisch synchronisiert wird
            -1 : keine automatische Synchronisation

             0 : andauernde automatische Synchronisation
        -- >

        < add key = ""SYNCHRONIZATION_WAITPERIOD"" value = ""3600"" />
   

           < !--Diese Objekte sollen nicht aus der DBGP gelöscht werden, auch wenn
               sie nicht mehr in der externen Datenbank vorhanden sind.

            Es sind nur Topobjekte zulässig.

            Mehrere Topobjekte können Pipe-getrennt angeben werden, z.B: value = ""prt|workcenter"".

        -- >

        < add key = ""SYNCHRONIZATION_NO_DELETE"" value = """" />
   

           < !--Wiederherstellungsmodus der Datenbank setzen, falls er nicht der Auswahl entspricht

            Mögliche Werte: [LEER], Full, Bulk-logged, or Simple

            Bei[Leer] wird keine Änderung versucht...
		-->
		<add key = ""DATABASE_RECOVERY_MODE"" value=""SIMPLE""/>
	</database_ganttplan>
	<database_extern>
		<!-- Material Example
        <add key= ""SQL_SELECT_ALL_material"" value= ""select * from MyMaterials"" />

        < add key= ""SQL_SELECT_ALL_material_routing"" value = ""NULL"" />

        < add key= ""SQL_SELECT_ALL_material_unitconversion"" value = ""NULL"" />
        -->

    </ database_extern >

    < xml_extern >

        < add key= ""PATH_XMLFILES""                    value= """" />

        < add key= ""SOURCEFILE_unit""                  value= """" />

        < add key= ""SOURCEFILE_material""              value= """" />

        < add key= ""SOURCEFILE_optimizationgroup""     value= """" />

        < add key= ""SOURCEFILE_workcenter""            value= """" />

        < add key= ""SOURCEFILE_workcentergroup""       value= """" />

        < add key= ""SOURCEFILE_worker""                value= """" />

        < add key= ""SOURCEFILE_workergroup""           value= """" />

        < add key= ""SOURCEFILE_prt""                   value= """" />

        < add key= ""SOURCEFILE_workingtimemodel""      value= """" />

        < add key= ""SOURCEFILE_shiftmodel""            value= """" />

        < add key= ""SOURCEFILE_shift""                 value= """" />

        < add key= ""SOURCEFILE_calendar""              value= """" />

        < add key= ""SOURCEFILE_basequalification""     value= """" />

        < add key= ""SOURCEFILE_routing""               value= """" />

        < add key= ""SOURCEFILE_bom""                   value= """" />

        < add key= ""SOURCEFILE_setupmatrix""           value= """" />

        < add key= ""SOURCEFILE_priority""              value= """" />

        < add key= ""SOURCEFILE_modelparameter""        value= """" />

        < add key= ""SOURCEFILE_planningparameter""     value= """" />

        < add key= ""SOURCEFILE_salesorder""            value= """" />

        < add key= ""SOURCEFILE_purchaseorder""         value= """" />

        < add key= ""SOURCEFILE_stock_quantityposting"" value= """" />

        < add key= ""SOURCEFILE_productionorder""       value= """" />

        < add key= ""SOURCEFILE_confirmation""          value= """" />

        < add key= ""SOURCEFILE_resultinfo""            value= """" />

        < add key= ""SOURCEFILE_resourcestatus""        value= """" />

        < add key= ""SOURCEFILE_idtemplate""            value= """" />

    </ xml_extern >

    < dll_extern >

        < !--die Schnittstellen-Dll -->
		<add key = ""PATH_DLL""                         value= """" />  < !--name or path to dll, e.g.value= ""connect.dll""-- >


        < !--die einzelnen Funktionsnamen -->
		<add key = ""FUNCTION_INIT""                    value= """" />

        < add key= ""FUNCTION_LOGIN""                   value= """" />

        < add key= ""FUNCTION_GETDATA""                 value= """" />

        < add key= ""FUNCTION_UPDATEDATA""              value= """" />

        < add key= ""FUNCTION_DELDATA""                 value= """" />

        < add key= ""FUNCTION_FREEBUFFER""              value= """" />

        < add key= ""FUNCTION_LOGOUT""                  value= """" />

        < add key= ""FUNCTION_EXIT""                    value= """" />


        < !--Fehlercodes-- >

        < !--enable / disable popup for errorcodes (default: true) -->
		<add key = ""ERRORCODE_SHOW"" value=""true""/>
		
		<!-- set error text for error code XY -->
		<!-- <add key = ""ERRORCODE_INFO_XY"" value=""Mein Fehler.""/>-->
		<!-- show popup for errorcode XY(default: true)-->
		<!-- <add key = ""ERRORCODE_SHOW_XY"" value=""false""/>-->
		
		<!-- company name the dll belongs to -->
		<add key = ""INTERFACE_VENDOR"" value=""""/>
		
		<!-- Loglevel bei Aufruf von DLL-Funktionen festlegen -->
		<!-- 0: kein spezielles Logging(nur Warnungen/Fehler werden geloggt) -->
		<!-- 1: wie 0, zusätzlich werden alle DLL-Funktionsaufrufe geloggt -->
		<!-- 2: wie 1, zusätzlich wird der Inhalt der Funktionsparamter beim Funktionsaufrufs und die erhaltenen Ergebnisdaten geloggt(kompletter Datenverkehr) -->
		<add key = ""LOGLEVEL"" value=""0""/>
	</dll_extern>
</configuration>
"},
                new Config { PropertyName = "interface.default.config", Value = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!-- Copyright © 2010-2018 DUALIS GmbH IT Solution - All Rights Reserved -->
<configuration>

    <version/>

    <default>

        <add key=""interface-import.default.config""/>

        <add key=""interface-export.default.config""/>

    </default>

    <api>

        <!-- Pfad und Name der zu ladenden Dll für kundenspezifischen Code (e.g. value=""project.dll"") -->

        <add key=""PATH_DLL"" value=""""/>

    </api>

    <application>

        <!-- General DB settings  -->

        <add key=""LOGIN_TIMEOUT_SECONDS"" value=""10""/>

        <add key=""QUERY_TIMEOUT_SECONDS"" value=""60""/>


        <!-- enable/disable parallel threads -->

        <add key=""USE_MULTITHREADS"" value=""true""/>

        <!-- enable/disable parallel database access -->

        <add key=""USE_MULTITHREADS_DB"" value=""true""/>


        <!-- collect and save object debug infos -->

        <add key=""SAVE_DEBUG_INFO"" value=""true""/>


        <!-- add custom environment variables to path

        e.g.: value = "".\java\jre\bin;.\java\jre\bin\server;""-- >

        < add key = ""ENVIRONMENT_VAR""  value = """" />
   

           < !--remove old system locks after this period of time-- >
   
           < add key = ""SYSTEMLOCK_REMOVE_SECONDS"" value = ""1800"" />
      

              < !--Welche Objekttypen sollen beim Übernehmen eines Szenarios beachtet werden? -->
      
              < !--Wenn keine Angabe, dann werden alle Objekte übernommen. Ansonsten können die gewünschten Objekttypen mittels 'Pipe' getrennt angegeben werden,

        < !--z.B.value = ""salesorder|productionorder"" wenn nur Bedarfe und Fertigungsaufträge aus dem Szenario übernommen werden sollen. -- >
 
         < add key = ""SCENARIO_APPLY_OBJECTTYPES"" value = """" />
    

            < !--Berechtigungen ""ignorieren""(funktioniert nur mit Dll - Datenquelle) und ""anonym"" für Fertigungssteuerer aktivieren / deaktivieren-- >
     
             < add key = ""PRODUCTIONCONTROLLER_PERMISSIONTYPE_IGNORE"" value = ""false"" />
        
                < add key = ""PRODUCTIONCONTROLLER_PERMISSIONTYPE_ANONYMOUS"" value = ""false"" />
           
               </ application >
           
               < datasource >
           
                   < !--wählbare Datenquellen für Systemdaten(Journaleinträge, Systemnutzer und Systemsperren): DBGP || Dll-- >
          
                  < add key = ""SYSTEMDATA"" value = ""DBGP"" />
             

                     < !--wählbare Datenquellen fuer Import: DBConfig || DBGP || Dll || NULL-- >
             
                     < add key = ""SQL_IMPORTSRC"" value = ""DBGP"" />
                

                        < !--Werks - und Fertigungssteuererinformationen standardmäßig nicht einlesen -->
                 
                         < add key = ""SQL_IMPORTSRC_plant"" value = ""NULL"" />
                    
                            < add key = ""SQL_IMPORTSRC_productioncontroller"" value = ""NULL"" />
                       

                               < !--abweichend vom Standard -->
                       
                               < !--SQL_IMPORTSRC_xxx, wobei xxx durch den Namen des Objekttyps ersetzt werden muss -->
                       
                               < !--Beispiele: -->
                       
                               < !-- < add key = ""SQL_IMPORTSRC_workcenter"" value = ""DBConfig"" /> -->
                           
                                   < !-- < add key = ""SQL_IMPORTSRC_resourcestatus"" value = ""NULL"" /> -->
                               

                                   </ datasource >
                               
                                   <interface>
		<!-- Es können optionale Aliasnamen für alle Eigenschaften gewünschter Objekttypen festgelegt werden -->
		<!-- Aliasnamen werden beim Import über DBConfig und XMLConfig sowie beim Export verwendet -->
		<!-- Hinweis: In Oracle-Datenbanken können nur Spaltennamen mit maximal 30 Zeichen verwendet werden.Dann müssen Aliasnamen verwendet werden -->
		<!-- Folgend sind alle Eigenschaften aufgezählt, bei denen der Name länger als 30 Zeichen ist -->		
		<!--
		<add key = ""ALIAS_material:purchase_time_quantity_dependent"" value=""purchase_time_quantity_dependent""/>
		<add key = ""ALIAS_material:purchase_time_quantity_independent"" value=""purchase_time_quantity_independent""/>
		<add key = ""ALIAS_workcenter:setup_static_time_needless_criteria"" value=""setup_static_time_needless_criteria""/>
		<add key = ""ALIAS_workcenter:setup_mandatory_optimization_criteria"" value=""setup_mandatory_optimization_criteria""/>
		<add key = ""ALIAS_workcentergroup_workcenter:workingtimemodel_workcentergroup_id"" value=""workingtimemodel_workcentergroup_id""/>
		<add key = ""ALIAS_prt:maintenance_interval_quantity_unit_id"" value=""maintenance_interval_quantity_unit_id""/>
		<add key = ""ALIAS_workergroup_worker:workingtimemodel_workergroup_id"" value=""workingtimemodel_workergroup_id""/>
		<add key = ""ALIAS_modelparameter:allow_overlap_activity_type_setup"" value=""allow_overlap_activity_type_setup""/>
		<add key = ""ALIAS_modelparameter:allow_overlap_activity_type_wait"" value=""allow_overlap_activity_type_wait""/>
		<add key = ""ALIAS_modelparameter:allow_change_worker_activity_time_min"" value=""allow_change_worker_activity_time_min""/>
		<add key = ""ALIAS_modelparameter:capital_commitment_interest_rate"" value=""capital_commitment_interest_rate""/>
		<add key = ""ALIAS_modelparameter:auto_confirm_child_productionorders"" value=""auto_confirm_child_productionorders""/>
		<add key = ""ALIAS_productionorder_operation_activity:info_date_earliest_start_initial"" value=""info_date_earliest_start_initial""/>
		<add key = ""ALIAS_productionorder_operation_activity:info_date_earliest_start_material"" value=""info_date_earliest_start_material""/>
		<add key = ""ALIAS_productionorder_operation_activity:info_date_earliest_end_material"" value=""info_date_earliest_end_material""/>
		<add key = ""ALIAS_productionorder_operation_activity:info_date_latest_start_material"" value=""info_date_latest_start_material""/>
		<add key = ""ALIAS_productionorder_operation_activity:info_date_earliest_start_scheduling"" value=""info_date_earliest_start_scheduling""/>
		<add key = ""ALIAS_productionorder_operation_activity:info_date_earliest_end_scheduling"" value=""info_date_earliest_end_scheduling""/>
		<add key = ""ALIAS_productionorder_operation_activity:info_date_latest_start_scheduling"" value=""info_date_latest_start_scheduling""/>
		<add key = ""ALIAS_productionorder_operation_activity:info_date_latest_end_scheduling"" value=""info_date_latest_end_scheduling""/>
		<add key = ""ALIAS_resultinfo:count_productionorder_incomplete"" value=""count_productionorder_incomplete""/>
		-->
	</interface>
    <idtemplates>
		<!-- <fn> = Freie Nummer(optional mit Angabe der Stellen) -->
		<!-- <sn> = Sequentielle Nummer(optional mit Angabe der Stellen) -->
		<!-- <y> = Jahr -->
		<!-- <m> = Monat -->
		<!-- <d> = Tag -->
		<!-- <h> = Stunde -->
		<!-- <mi> = Minute -->
        <add key = ""default"" value=""<fn>""/>
		<add key = ""productionorder"" value=""<sn6>""/>
		<add key = ""purchaseorder"" value=""<sn6>""/>
    </idtemplates>
</configuration>
"},
                new Config { PropertyName = "local.config", Value = @"﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿<?xml version=""1.0"" encoding=""UTF-8""?>
<configuration>

    <version/>

    <redirect>

        <add key=""*"" value=""C:\Program Files\GANTTPLAN\Config\GanttPlan.config"" />

    </redirect>
</configuration>
"},
                new Config { PropertyName = "opt.default.config", Value = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!-- Copyright © 2010-2018 DUALIS GmbH IT Solution - All Rights Reserved -->
<configuration>

    <version/>

    <application>

        <!-- report decimal digits -->

        <add key=""REPORT_DIGITS"" value=""2""/>

        <!-- report shows formula error messages -->

        <add key=""REPORT_FORMULA_SHOW_ERROR"" value=""true""/>


        <!-- time buffer in report 'sales orders overview' (in seconds) -->

        <add key=""REPORT_SALESORDERS_OVERVIEW_TIMEBUFFER"" value=""604800""/>

        <!-- convert quantities into this unit in all reports of type production_quantities_... -->

        <!-- <add key=""REPORT_PRODUCTION_QUANTITIES_UNIT_ID"" value=""kg""/> -->


        <!-- respected interval types in utilization calculation (e.g. if blocked and reserved intervals should not be considered you have to remove these entries) -->

        < !-- < add key = ""REPORT_UTILIZATION_TYPES"" value = ""PROCESSING|SETUP|WAITING|EXTERNAL|RESERVED|BLOCKED"" /> -->
    
        </ application >
    
        < project >
    
            < !--Sicherheitsbestand während des MRP beachten -->
    
            < !--true: Sicherheitsbestand / Sperrbestand bei MRP beachten-- >
      
              < !--false: Sicherheitsbestand / Sperrbestand bei MRP nicht beachten(obwohl am Material angegeben) -->
       
               < add key = ""MRP_CHECK_SICHERHEITSBESTAND"" value = ""true"" />
          
                  < !--true: Die Menge in allen Materialbeziehungen(fixierte und nicht fixierte) wird zu Beginn des MRP automatisch auf die undefinierte Menge(-1.0) gesetzt-- >
        
                < !--false: Die Menge in Materialbeziehungen wird nicht automatisch verändert, es gilt der importierte Wert-- >
        
                < add key = ""MRP_RESET_LINKMENGE"" value = ""false"" />
           
                   < !--true: fremdbezogenes Material so früh wie möglich verbrauchen-- >
           
                   < !--false: fremdbezogenes Material so früh wie nötig verbrauchen-- >
           
                   < add key = ""MRP_FIFO_FB"" value = ""false"" />
              
                      < !--true: Vermeidung zu früher unvollständiger Reservierungen für eigengefertigtes Material(frühere Reservierungen freigegeben, wenn sowieso auf spätere Reservierung gewartet werden muss) -->
             
                     < !--false: unvollständige Reservierungen für eigengefertigtes Material werden genutzt-- >
             
                     < add key = ""MRP_AVOID_EARLY_INCOMPLETE_RESERVATIONS_EF"" value = ""true"" />
                
                        < !--true: Vermeidung zu früher unvollständiger Reservierungen für fremdbezogenes Material(frühere Reservierungen freigegeben, wenn sowieso auf spätere Reservierung gewartet werden muss) -->
               
                       < !--false: unvollständige Reservierungen für fremdbezogenes Material werden genutzt-- >
               
                       < add key = ""MRP_AVOID_EARLY_INCOMPLETE_RESERVATIONS_FB"" value = ""true"" />
                  
                          < !--true: fremdbezogenes Material anhand Planungsergebnis neu reservieren/ terminieren-- >
                  
                          < !--false: fremdbezogenes Material anhand MRP reservieren / terminieren-- >
                  
                          < !--Hinweis: Wenn aktiviert, passt die Fremdbezug-Materialverteilung besser zum Ergebnis, erschwert aber die Analyse der Planungsursachen -->
                  
                          < add key = ""MRP_OPT_FB"" value = ""false"" />
                     
                             < !--Modus für die Umterminierung von Bestellvorschlägen, falls MRP_OPT_FB nicht aktiviert ist-- >
                     
                             < !--0: Bestelltermin so früh wie möglich-- >
                     
                             < !--1: Bestelltermin auf Basis der initialen Grobplanung bestimmen(eigengefertigtes Material hat auch Einfluss auf den Bestelltermin)-- >
                     
                             < !--2: Bestelltermin auf Basis der Grobplanung mit fremdbezogenem Material bestimmen(anderes fremdbezogenes Material hat auch Einfluss auf den Bestelltermin)-- >
                     
                             < !--3: Bestelltermin auf Basis des tatsächlich geplanten Starts bestimmen -->
                     
                             < add key = ""MRP_FB_DUEDATE_MODE"" value = ""1"" />
                        
                                < !--Anfangsbestand-- >
                        
                                < !--Der Material - Verbrauch / Input welcher Vorgangs-Status ist bereits im Anfangsbestand enthalten: GESTARTET | TRUECK | BEENDET-- >
                            
                                    < add key = ""MRP_ANFBESTAND_VERBRAUCH_STATUS"" value = ""GESTARTET|TRUECK|BEENDET"" />
                               
                                       < !--Verbrauchsmenge laut Vorgang/ FA-- >
                               
                                       < !--true: Die Bruttomenge am FA gibt an, für welche verbleibende Menge die Verbrauchsmengen aller Vorgänge des FAs zu reservieren sind. -->
                               
                                       < !--false / default: Die Bruttomenge am Vorgang gibt an, für welche Menge die Verbrauchsmengen des Vorgangs zu reservieren sind. -- >
                                 
                                         < add key = ""MRP_VERBRAUCH_MENGE_FAKOPF"" value = ""false"" />
                                    
                                            < !--Der Material - Zugang / Output welcher Vorgangs-Status ist bereits im Anfangsbestand enthalten: GESTARTET | TRUECK | BEENDET-- >
                                        
                                                < add key = ""MRP_ANFBESTAND_ZUGANG_STATUS"" value = ""BEENDET"" />
                                           
                                                   < !--Soll die rückgemeldete produzierte Menge(bereits hergestelltes Material) als im Anfangsbestand enthalten angenommen werden? -->
                                          
                                                  < !--true: Rückgemeldete Menge wurde schon im Anfangsbestand eingebucht(ist dort also schon enthalten) und wird somit nicht nochmal zum Zeitpunkt der Fertigstellung in den Bestand eingebucht. -- >
                                         
                                                 < !--false: Rückgemeldete Menge ist noch nicht im Anfangsbestand enthalten. Die gesamte Menge wird somit erst zum Zeitpunkt der Fertigstellung in den Bestand eingebucht. -- >
                                         
                                                 < add key = ""MRP_CONFIRMED_QUANTITY_IN_INITAL_STOCK"" value = ""false"" />
                                            
                                                    < !--Zugangsmenge laut Vorgang/ FA-- >
                                            
                                                    < !--true / Sonderfall: Die Nettomenge am FA gibt an, welche verbleibende Menge von den letzten Vorgängen geliefert wird. Nur zu verwenden, wenn es immer eindeutigen letzten Vorgang gibt!-->
                                              
                                                      < !--false / default: Die Nettomenge an letzten Vorgängen gibt an, welche Menge geliefert wird. -->
                                                
                                                        < add key = ""MRP_ZUGANG_MENGE_FAKOPF"" value = ""false"" />
                                                   
                                                           < !--FAs, deren hergestelltes Material nicht benötigt wird, bekommen als Termin PZR - Ende-- >
                                                   
                                                           < !--true / Sonderfall: PZR - Ende für alle FAs, welche keinen Primärauftrag / Bedarf decken, keinen negativen Anfangsbestand auffüllen und auch keinen Sicherheits/ Sperrbestand decken
                                                                  < !--false / default: nicht benötigte FAs bekommen nicht zwangsweise PZR - Ende als Termin gesetzt-- >
                                                              
                                                                      < add key = ""MRP_PZRENDE_NICHT_BENOETIGTE_FA"" value = ""false"" />
                                                                 
                                                                         < !--Sichtbarkeit der während des MRP automatisch angelegten Bedarfe zur Deckung des Sicherheitsbestands-- >
                                                                 
                                                                         < !--true / Sonderfall: während MRP automatisch erstelle Bedarfe um Sicherheitsbestand zu decken bleiben erhalten-- >
                                                                   
                                                                           < !--false / default: Sicherheitsbestandsbedarfe werden am Ende des MRPs wieder gelöscht -->
                                                                     
                                                                             < add key = ""MRP_BEHALTE_SB_BEDARFE"" value = ""false"" />
                                                                        
                                                                                < !--Dürfen sich Prognosebedarfe mit Sicherheitsbestands-und Lagerbedarfen verrechnen? -->
                                                                       
                                                                               < !--true / Sonderfall: Sicherheitsbestands - und Lagerbedarfe dürfen die prognostizierte Menge reduzieren -->
                                                                          
                                                                                  < !--false / default: Sicherheitsbestands - und Lagerbedarfe dürfen die prognostizierte Menge nicht reduzieren-- >
                                                                             
                                                                                     < add key = ""MRP_REDUZIERE_PB_MIT_SB"" value = ""false"" />
                                                                                
                                                                                        < !--Modus zur Auswahl(binär kombinierbar), welche ausgehenden Materialbeziehungen Einfluss auf die automatische Anpassung von Termin, Priorität und Planungstyp haben-- >
                                                                               
                                                                                       < !--0: alle Verursacher haben Einfluss auf Termin, Priorität und Planungstyp -->
                                                                               
                                                                                       < !--1: Bedarfe in Materialbeziehung mit Menge kleiner-gleich 0 haben keinen Einfluss-- >
                                                                               
                                                                                       < !--2: Kundenbedarfe priorisiert betrachten(falls Materialbeziehungen zu echten Kundenbedarfen vorhanden, dann werden nur diese beachtet, andere Bedarfstypen haben dann keinen Einfluss)-- >
                                                                               
                                                                                       < add key = ""MRP_AUTOUPDATE_MODE"" value = ""0"" />
                                                                                  
                                                                                          < !--Werden die Materialbeziehungen von fixiert oder stabil zu planenden Vorgängen flexibel behandelt ? -->
                                                                                  
                                                                                          < !--true / Sonderfall : Materialbeziehungen von fixiert oder stabil zu planenden Vorgängen werden neu ermittelt, falls flexible Beziehungen aktiviert-- >
                                                                                     
                                                                                             < !--false / default: Materialbeziehungen von fixiert oder stabil zu planenden Vorgängen werden nicht neu ermittelt, lediglich deren Menge darf angepasst werden-- >
                                                                                       
                                                                                               < add key = ""MRP_RELINK_FIXSTART"" value = ""false"" />
                                                                                          
                                                                                                  < !--Sollen vorhandene Materialbeziehungen ohne echte Menge nach Abschluss des MRPs automatisch gelöscht werden? Dies betrifft auch fixierte Materialbeziehungen sowie Beziehungen von stabil / fixiert geplanten Vorgängen -->
                                                                                            
                                                                                                    < !--true / Sonderfall: Materialbeziehungen ohne echte Menge werden automatisch gelöscht-- >
                                                                                              
                                                                                                      < !--false / default: Materialbeziehungen ohne echte Menge bleiben erhalten -->
                                                                                                
                                                                                                        < add key = ""MRP_DELETE_RELATIONS_WITHOUT_QUANTITY"" value = ""false"" />
                                                                                                   
                                                                                                           < !--Wie sollen Bedarfe zum Auffüllen des Sicherheitsbestands behandelt werden? -->
                                                                                                   
                                                                                                           < !--true / default: Sie dürfen sich erst benötigtes Verbrauchsmaterial reservieren, wenn alle anderen Objekte damit fertig sind.Somit können sie keinen anderen Objekten Material zu früh ""wegnehmen"". -- >
                                                                                                    
                                                                                                            < !--false / Sonderfall: Sie werden gleichwertig gegenüber allen anderen Objekten bei der Reservierung von Verbrauchsmaterial angesehen.Somit können diese Bedarfe ihr Verbrauchsmaterial früher reservieren und dieses auch anderen Objekten wegnehmen. -->
                                                                                                     
                                                                                                             < add key = ""MRP_SAFETYSTOCK_REFILL_LAST"" value = ""true"" />
                                                                                                        

                                                                                                                < !--MRP Sortieroptionen-- >
                                                                                                        
                                                                                                                < !--Übersicht der möglichen Sortierkriterien-- >
                                                                                                        
                                                                                                                < !--Sortierkriterien können komma-getrennt angegeben werden. Zwei Objekte werden in der Reihenfolge der angegeben Sortierkriterien verglichen. -->
                                                                                                        
                                                                                                                < !--Sobald sich zwei Objekte bzgl. eines Sortierkriteriums unterscheiden, wird dieses Sortierkriterium für den Vergleich verwendet. -- >
                                                                                                        
                                                                                                                < !--Damit ein deterministisches Verhalten sichergestellt ist, sollte schlussendlich immer nach Objekt - ID und Objekttyp verglichen werden. -->
                                                                                                          
                                                                                                                  < !--Folgende Kriterien sind nur für Materialbereitsteller möglich -->
                                                                                                          
                                                                                                                  < !--0: Materialbereitsteller aufsteigend sortiert nach Buchungszeitpunkt-- >
                                                                                                       
                                                                                                               < !--1: Materialbereitstellung aus Lager wichtiger als andere Materialbereitsteller-- >
                                                                                                    
                                                                                                            < !--2: Materialbereitstellung von Bestellung wichtiger als andere Materialbereitsteller-- >
                                                                                                 
                                                                                                         < !--3: Materialbereitstellung von Fertigungsauftrag / Vorgang wichtiger als andere Materialbereitsteller -->
                                                                                                
                                                                                                        < !--Folgende Kriterien sind für Materialbereitsteller und - verbraucher möglich-- >
                                                                                                 
                                                                                                         < !--9:  aufsteigend sortiert nach Anzahl noch offener(nicht begonnener) Vorgänge in einem Auftrag(Fortschritt des Auftrags) -->
                                                                                            
                                                                                                    < !--10: aufsteigend sortiert nach Anzahl noch offener(nicht begonnener) Vorgänge in einem mindestens gestarteten Auftrag(Fortschritt des Auftrags) -->
                                                                                       
                                                                                               < !--11: benötigte Aufträge sind wichtiger als nicht benötigte-- >
                                                                                    
                                                                                            < !--12: aufsteigend sortiert nach frühstem Start eines stabilen oder fixierten Vorgangs des Auftrags(Aufträge mit stabilen oder fixierten Vorgängen sind somit immer wichtiger als Aufträge ohne diese Vorgänge) -->
                                                                                
                                                                                        < !--13: aufsteigend sortiert nach Auftragsstart(gestartete Aufträge sind somit auch wichtiger als nicht gestartete Aufträge) -->
                                                                            
                                                                                    < !--14: gestartete Aufträge sind wichtiger als nicht gestartete Aufträge
                                                                                 <!--20: aufsteigend sortiert nach Vorgangsstart eines stabilen oder fixierten Vorgangs(stabile oder fixierte Vorgänge sind somit immer wichtiger als nicht stabile oder fixierte Vorgänge)-- >
                                                                     
                                                                             < !--21: aufsteigend sortiert nach Vorgangsstart eines mindestens gestarteten Vorgangs(gestartete Vorgänge sind somit auch wichtiger als nicht gestartete Vorgänge) -->
                                                                 
                                                                         < !--22: gestartete Vorgänge sind wichtiger als nicht gestartete Vorgänge
                                                                      <!--30: sortiert nach Planungstyp(Optimierung / Anfrage sind wichtiger als Zusatzanfragen)-- >
                                                          
                                                                  < !--31: aufsteigend sortiert nach Prioritätswert(Rangfolge) -->
                                                      
                                                              < !--32: aufsteigend sortiert nach anfallenenden Verspätungskosten-- >
                                                   
                                                           < !--33: aufsteigend sortiert nach Termin -->
                                                
                                                        < !--34: nach Primärbedarfstyp sortiert(echte Kundenbedarfe sind wichtiger als Prognose - und Lagerbedarfe)-- >
                                             
                                                     < !--40: sortiert nach Objekt - ID(lexikographisch)-- >
                                          
                                                  < !--41: sortiert nach Objekttyp(lexikographisch)-- >
                                       

                                               < !--Für einfacher Anpassung: Übersetzung alter MRP - Sortiermodi in die Schreibweise der neuen Sortierkriterien-- >
                                         
                                                 < !--MRP_LINKMODE 0: Materialverknüpfung anhand der Kombination[Termin, Fortschritt] vom Verbraucher entscheiden(bei gleichem Termin ist der Verbraucher wichtiger, welcher weniger noch zu planende Vorgänge hat, also weiter fortgeschritten ist)-- >
                                        
                                                < !--MRP_LINKMODE 1: Materialverknüpfung anhand der Kombination[Fix / Stabil - Start, Planungstyp, Priorität, Termin, Fortschritt] vom Verbraucher entscheiden-- >
                                           
                                                   < !--MRP_LINKMODE 2: Materialverknüpfung anhand der Kombination[Bedarfstyp, Termin, Fortschritt] entscheiden.Echte Kundenbedarfe sind wichtiger als Lager-und Prognosebedarfe.Bei gleicher Wichtigkeit entscheidet der Termin/ Fortschritt wie bei Modus 0-- >
                                        
                                                < !--MRP_LINKMODE 3: Materialverknüpfung anhand der Kombination[Fix / Stabil - Start, Termin, Fortschritt] vom Verbraucher entscheiden-- >
                                           
                                                   < !--früher 0: 11,33,10,40,41-- >
                                           
                                                   < !--früher 1: 11,13,12,30,31,32,33,10,40,41-- >
                                           
                                                   < !--früher 2: 11,34,33,10,40,41-- >
                                           
                                                   < !--früher 3: 11,13,12,33,10,40,41-- >
                                           

                                                   < !--Sortierkriterien für Materialverbraucher bei der Eigenfertigung(komma - getrennt die Sortierkriterien angeben, in welcher Reihenfolge verglichen werden soll)-- >
                                           
                                                   < add key = ""MRP_SORT_CONSUMER_INHOUSEPRODUCTION"" value = ""11,33,10,40,41"" />
                                              
                                                      < !--Sortierkriterien für Materialverbraucher beim Fremdbezug(komma-getrennt die Sortierkriterien angeben, in welcher Reihenfolge verglichen werden soll) -->
                                             
                                                     < add key = ""MRP_SORT_CONSUMER_PURCHASE"" value = ""11,13,12,30,31,32,33,10,40,41"" />
                                                

                                                        < !--Übersicht der Werte für die Angabe des maximalen Suchezeitpunkts im Bestandsverlauf -->
                                                
                                                        < !--0: Suche ab Ist - Linie bis zum Buchungszeitpunkt(Zeitpunkt ab wann das Material bereitsteht)-- >
                                               
                                                       < !--1: Suche ab Ist - Linie bis zur maximalen Wartezeit(falls angegeben, sonst bis zum Planungszeitraumende) -->
                                             
                                                     < !--2: Suche ab Ist - Linie bis zum Planungszeitraumende-- >
                                            

                                                    < !--Übersicht erlaubten Materialbereitsteller(Werte dürfen auch binär kombiniert angegeben werden) -->
                                           
                                                   < !--1: Lagerbestand-- >
                                        
                                                < !--2: Lieferantenbestellung-- >
                                     
                                             < !--4: Bedarf-- >
                                  
                                          < !--8: Fertigungsauftragsvorgang selbst ist bde-gemeldet, start - fixiert oder stabil geplant-- >
                                 
                                         < !--16: Fertigungsauftragsvorgang selbst ist bde-gemeldet oder Fertigungsauftrag ist bde-gemeldet und Vorgang ist entweder start - fixiert oder stabil geplant-- >
                                
                                        < !--32: Fertigungsauftrag ist bde - gemeldet-- >
                             
                                     < !--64: Fertigungsauftrag ist offen-- >
                          
                                  < !----Beispielhafte Binärkombination--
                               < !--3: Terminsichere Materialbereitsteller(Binärkombination von Lagerbestand und Lieferantenbestellung)

        < !--120: Terminunsichere Materialbereitsteller(Binärkombination der restlichen Materialbereitsteller)

        < !--127: Alle(Binärkombination aller Werte)

     < !--Durch | getrennt können mehrere Regelsätze angegeben werden, nach welchen ein gültiger Materialbereitsteller gesucht wird-- >

     < !--Durch; getrennt werden pro Regelsatz drei Werte: [maximaler Suchezeitpunkt];[erlaubte Materialbereitsteller];[Sortierkriterien] -->
		<!-- Durch , getrennt werden die erlaubten Materialbereitsteller und Sortierkriterien -->
		<!-- Es dürfen beliebig viele Regelsätze definiert werden.Sobald in einem Regelsatz ein gültiger Materialbereitsteller gefunden wurde bricht die Suche ab. -->
		
		<!-- Algorithmus für die Suche nach Materialbereitstellern für die sortierten Materialverbraucher bei der Eigenfertigung -->
		<add key = ""MRP_SORT_PROVIDER_INHOUSEPRODUCTION"" value=""0;1,2,8,16;0,40,41|2;1,2,16,32;0,40,41|1;127;0,40,41"" />
		
		<!-- Algorithmus für die Sortierung von Materialbereitstellern aus Beziehungen ohne Mengenangabe(undefinierte Menge) -->
		<add key = ""MRP_SORT_PROVIDER_UNDEFINED_QUANTITY_LINKS"" value=""2;127;1,2,22,14,33,40,41"" />
		
		<!-- Automatisch berechnete Sekundärauftragstermine nicht vor frühestem realisierbaren Ende? -->
		<add key = ""OPT_AUTO_DUEDATES_REALIZABLE"" value=""false""/>
		
		<!-- Losgrößenoptimierung -->
		<!-- Terminabstand(in Sekunden) von FAs welche noch zusammengefasst werden dürfen -->
		<add key = ""LOSOPT_ZEITHORIZONT"" value=""1209600"" />
		<!-- Kriterium für Bewertungsfunktion, welche ermittelt welche FAs am besten zusammengefasst werden können -->
		<!-- 0: ähnlicher Termin und Erreichen optimaler Losgröße gleich wichtig -->
		<!-- 1: ähnlicher Termin wichtiger als Erreichen optimaler Losgröße -->
		<!-- 2: Erreichen optimaler Losgröße wichtiger als ähnlicher Termin -->
		<add key = ""LOSOPT_SORTMODE"" value=""0"" />
		<!-- true: ein Auftrag wird nicht mit einem anderen Auftrag zusammengefasst, wenn er sich dadurch verspäten würde -->
		<!-- false: zusätzliche Verspätung wird nicht beachtet -->
		<add key = ""LOSOPT_VERSPAETUNG_PRUEFEN"" value=""true"" />
		
		<!-- Soll ein Fertigungsauftrag automatisch freigegeben werden, wenn dieser manuell erstellt, geändert oder im Leitstand modifiziert wurde? -->
		<!-- true/default : Fertigungsauftrag wird freigegeben -->
		<!-- false: Fertigungsauftrag wird nicht freigegeben -->
		<add key = ""PRODUCTIONORDER_APPROVE_AUTO"" value=""true"" />
		
		<!-- Strategie zur Einplanungsfreigabe von Vorgängen während der automatischen Planung -->
		<!-- VORGANGEINZELN: Freigabe wenn alle Materialabhängigkeiten des Vorgangs geplant sind -->
		<!-- FAKOMPLETT: Freigabe wenn alle Materialabhängigkeiten des gesamten FAs geplant sind -->
		<add key = ""OPT_PLANUNGSFREIGABE"" value=""VORGANGEINZELN"" />
		
		<!-- Belegungsreihenfolge auf Arbeitsplätzen -->
		<!-- Strenges Überholverbot auf Arbeitsplatzen: Lücken vor bereits geplanten Vorgängen werden nicht aufgefüllt -->
		<!-- *: Überholverbot auf allen Arbeitsplätzen -->
		<!-- |: Aufzählen der Arbeitsplatz-IDs, getrennt mit | -->
		<add key = ""SCHEDULE_ORDER_BACK_WORKCENTER"" value=""""/>
		<!-- Strenges Überholverbot auf Arbeitsplätzen im stabilen Zeitraum: Lücken vor bereits geplanten Vorgängen werden nicht aufgefüllt -->
		<add key = ""SCHEDULE_ORDER_STABLE"" value=""true"" />
		<!-- Anpassung des Überholverbots im stabilen Zeitraum und bei Korrekturplanen/Lücken Schließen für Arbeitsplätze ohne Kapazitätsprüfung -->
		<!-- 0: Ein Überholverbot gilt auch für Arbeitsplätze ohne Kapazitätsprüfung -->
		<!-- 1: Ein Überholverbot gilt nicht für Arbeitsplätze ohne Kapazitätsprüfung -->
		<add key = ""SCHEDULE_ORDER_STABLE_TYPE"" value=""0"" />
		<!-- Freigegebene FA vor nicht freigegebenen FA auf Arbeitsplätzen -->
		<add key = ""SCHEDULE_ORDER_APPROVED"" value=""false"" />
		
		<!-- Belegungsreihenfolge auf FHM -->
		<!-- Strenges Überholverbot auf FHM: Lücken vor bereits geplanten Vorgängen werden nicht aufgefüllt -->
		<!-- *: Überholverbot auf allen FHM -->
		<!-- |: Aufzählen der FHM-IDs, getrennt mit | -->
		<add key = ""SCHEDULE_ORDER_BACK_PRT"" value=""""/>
		
		<!-- dürfen Vorgänge, welche zuvor nicht im stabilen Zeitraum lagen, diesen auffüllen -->
		<!-- true/default: zuvor nicht im stabilen Zeitraum geplante Vorgänge dürfen in den stabilen Zeitraum geplant werden -->
		<!-- false/Sonderfall: zuvor nicht im stabilen Zeitraum geplante Vorgänge dürfen erst ab Ende stabilen Zeitraum eingeplant werden(ausgenommen manuell verschobene Vorgänge) -->
		<add key = ""STABIL_ZEITRAUM_AUFFUELLEN"" value=""true"" />
		<!-- Soll die Startzeit von im stabilen Zeitraum liegenden Vorgängen beibehalten werden(weiche Schranke) -->
		<!-- true/default: Es wird versucht diese Vorgänge exakt an die gleiche Position wie zuvor zu planen -->
		<!-- false/Sonderfall: Diese Vorgänge dürfen neue Planzeiten bekommen(stabiler Zeitraum wird von vorn aufgefüllt) -->
		<add key = ""STABIL_START_FIXIERT"" value=""true"" />
		<!-- Sollen Pausen aus der Dauer des stabilen Zeitraums rausgerechnet werden -->
		<add key = ""STABIL_ZEITRAUM_AZM"" value=""true"" />		
		<!-- Stabile Reihenfolge verletzen, um Restriktionenen MaxPuffer, Job, sequentielle Linie oder Reservierung in der stabilen Planung einzuhalten? -->
		<add key = ""STABIL_RESTRIKTIONEN_PRUEFEN"" value=""true""/>

		<!-- alternative Vorgänge werden geplant -->
		<add key = ""PLANUNG_ALTERNVORGANG"" value=""true"" />
		<!-- Vorgänge werden gesplittet, falls definiert -->
		<add key = ""PLANUNG_VORGANGSPLITTEN"" value=""true"" />
		<!-- Pausen werden bei der Überlappungsberechnung einbezogen ja/nein, falls ja besteht die Gefahr das der Nachfolger ""leerläuft"" -->
		<add key = ""PLANUNG_UEBERLAP_PAUSEN"" value=""false"" />
		
		<!-- Engpassterminierung: spätestens zum(spätesten Start - Wert_ZF_Termin* minimalen Puffer) starten -->
		<add key = ""OPT_VORWAERTSRESERVEZEITMIN"" value=""259200"" />
		<!-- Engpassterminierung: frühestens zum(spätesten Start - maximalen Puffer) starten -->
		<add key = ""OPT_VORWAERTSRESERVEZEITMAX"" value=""2592000"" />
		
		<!-- Berechnung des Planungsstatus(ok, kritisch, verspätet, nicht eingeplant) abhängig von Parent-FAs
       <!-- true/Sonderfall: Der Planungsstatus eines Sekundärauftrages erbt den Planungsstatus des Primärauftrags. -->
		<!-- false/default: Der Planungsstatus eines Sekundärauftrages ist nicht abhängig von Primäraufträgen.Er wird eigenständig ermittelt. -->
		<add key = ""OPT_SCHEDULINGSTATUS_FROM_PARENTS"" value= ""false"" />


       < !--welche Sortierkriterien sollen beachtet/genutzt werden -->	
		<add key = ""APLANCACHE_KRITERIEN_APLANCACHE_PLANUNGSTYP"" value= ""true"" />

       < add key= ""APLANCACHE_KRITERIEN_APLANCACHE_BDESTATUS_FA"" value= ""false"" />

       < add key= ""APLANCACHE_KRITERIEN_APLANCACHE_STATUS_FA"" value= ""false"" />

       < add key= ""APLANCACHE_KRITERIEN_APLANCACHE_PRIO"" value= ""true"" />

       < add key= ""APLANCACHE_KRITERIEN_APLANCACHE_PRIO_VORGANG"" value= ""true"" />

       < add key= ""APLANCACHE_KRITERIEN_APLANCACHE_STATUS_FA_NETZWERK"" value= ""false"" />

       < add key= ""APLANCACHE_KRITERIEN_APLANCACHE_SPLIT"" value= ""false"" />


       < !--Auswahltiefe einschränken -->
		<add key = ""APLANCACHE_VORAUSWAHL"" value= ""-1"" />

       < add key= ""APLANCACHE_NACHAUSWAHL"" value= ""-1"" />


       < !--Maximale Anzahl von Detailprüfungen pro Rüstoptimierungsschritt (0 = unbeschränkt) -->
		<add key = ""OPT_SETUPOPT_EVALMAX"" value=""5"" />
		
		<!-- Grober Zeithorizont für ideale Rüstgruppierung; beeinflusst Planung; keine harte Ergebnisregel -->
		<add key = ""OPT_CLUSTER_RUEST"" value=""1209600"" /> 		<!-- 14 Tage: max.Abstand SStart für Gruppierung -->
				
		<!-- harte zeitschranken für Rüst/Belegungs-Opt in Sekunden-->
		<add key = ""OPT_RUESTMAXFRUEH"" value=""-1"" />
		<add key = ""OPT_RUESTMAXVERSP"" value=""-1"" />

		<!-- Ruest/Belegungszwang ordnet sich der harten Prioritäten unter? ja/nein -->
		<add key = ""OPT_ZWANG_PRIO"" value=""true"" />

		<!-- Experimentell: Maximale Dauer einer Pause zwischen zwei Bearbeitungsintervallen eines Personals an einem Vorgang -->
		<!-- bevor dies als Unterbrechung gewertet wird und die Prüfung auf die minimale Bedienlänge ohne Unterbechung neu startet -->
		<add key = ""OPT_PERSONAL_MAXPAUSE"" value=""-1"" />
		
		<!-- automatisch Rüstpositionen für dynamisches Rüsten anlegen -->
		<add key = ""OPT_RUESTPOS_AUTOCREATE"" value=""true"" />
		
		<!-- Sollen immer auftragsbezogene Arbeitspläne/Stücklisten bei Fertigungsaufträgen verwendet werden? -->
		<!-- true: Es werden automatisch immer auftragsbezogene Arbeitspläne/Stücklisten erstellt/verwendet. -->
		<!-- false: Es werden automatisch nur dann auftragsbezogene Arbeitspläne/Stücklisten erstellt/verwendet, wenn diese sich von ihrer Vorlage unterscheiden. -->
		<add key = ""OPT_ALWAYS_ORDER_RELATED_ROUTING_BOM"" value=""false"" />
		
		<!-- automatische Anpassung der Bedarfstermine für Demodatenbanken-->
		<add key = ""DEMO_BEDARFSTERMINE"" value=""false""/>
		
		<!-- Regel für die Interpretation der alternativen FHM-Gruppen im Arbeitsplan -->
		<!-- 0: Alle FHMs aus einer Gruppe sind zu verwenden(ganze FHM-Gruppen sind zueinander alternativ). -->
		<!-- 1: Ein FHM aus jeder Gruppe ist zu verwenden.Die Auswahl des gleichen FHMs aus mehreren Gruppen ist nicht erlaubt. -->
		<!-- 2: Ein FHM aus jeder Gruppe ist zu verwenden. Die Auswahl des gleichen FHMs aus mehreren Gruppen ist erlaubt. Die resultierende Belegung ist die Summe der Einzelbelegungen der gleichen FHMs. -->
		<!-- 3: Ein FHM aus jeder Gruppe ist zu verwenden. Die Auswahl des gleichen FHMs aus mehreren Gruppen ist erlaubt. Die resultierende Belegung ist die des FHMs mit der größten Belegung. -->
		<add key = ""PRT_GROUP_TRANSFORM_TYPE"" value= ""0"" />


        < !--Maximale Zeit in Sek.zwischen Vorgaenger und Rüstoptimierten Nachfolger auf Maschine / Ab welcher Zeitspanne gilt Maschine als ungerüstet / (-1==inf) -->
		<add key = ""OPT_RUESTMAXRESERVE"" value=""3600""/>
		<!-- Lücke zwischen Vorgaenger und Rüstoptimierten Nachfolger auf Maschine wird blockiert(reserviert) -->
		<add key = ""OPT_RUESTMAXRESERVE_BLOCKIEREN"" value=""true""/>
		
		<!-- Regel für Ermittlung des Rüstzustandes eines Arbeitsplatzes innerhalb des reservierten Zeitraumes(siehe OPT_RUESTMAXRESERVE) -->
		<!-- false/default: Der Rüstzustand wird immer anhand des direkten Vorgängers auf dem Arbeitsplatz ermittelt. -->
		<!-- true: Der Rüstzustand wird anhand des letzten Vorgängers auf dem Arbeitsplatz, der eine Rüstaktivität mit potentieller Dauer/Kosten hat, ermittelt. -->
		<add key = ""OPT_KEEP_SETUP_WORKCENTER"" value= ""false"" />


        < !--Ergebnisse aus späteren Optimierungsläufen darf bestes Ergebnis werden? -->
		<add key = ""OPT_RESULT"" value= ""true"" />


        < !--Intern: Verfahrensauswahl für initiale Planreihenfolge (0:Vorgangstermin,1:Auftragstermin,2:Vorgangstermin+Verspätungskosten,3:Auftragstermin+frühster Start)-->
		<add key = ""OPT_QUEUE_INITMODE"" value=""2""/>
		<!-- Kriterium zur Initialisierung der deterministischen Sortierung von Ressourcen, true: Auftragsnummer, false: Aktivitätssnummer -->
		<add key = ""OPT_BALANCE_PER_ORDER"" value=""true""/>
		
		<!-- true: Bei Unterbrechung wird ein neuer Splitt mit der bereits gefertigten Menge angelegt.Der Vorgang selbst enthält nur noch die zu fertigende Restmenge. -->
		<!-- false: Es wird kein Splitt mit der bereits gefertigten Menge angelegt (beendeter Anteil wird vergessen). Der Vorgang selbst enthält nur noch die zu fertigende Restmenge. -->		
		<add key = ""BDE_UNTERBROCHEN_SPLITT"" value= ""true"" />


        < !--Der Status des noch zu fertigenden Anteils nach einer Unterbrechung.Mögliche Werte sind: UNGEPLANT, GEPLANT, GESTARTET -->
		<add key = ""BDE_UNTERBROCHEN_STATUS_RESTANTEIL"" value= ""UNGEPLANT"" />


        < !--Terminierung eines unbekannten (zuvor nicht gemeldeten) Vorgangsstarts beim automatischen Rückmelden -->
		<!-- true/default: Start wird anhand des gemeldeten Ende automatisch terminiert(Start = Ende - Basisdauer des Vorgangs) -->
		<!-- false/Sonderfall: Start wird auf Ende gesetzt, Vorgang hat keine Zeit verbraucht -->
		<add key = ""BDE_AUTORUECKMELDEN_AUTOTERMINIEREN"" value=""true""/>
		
		<!-- Soll ein BDE gemeldeter Vorgang zusammenhängend geplant werden oder darf eine mindestens gestartete Rüstposition aus einem Vorgang separat vorgezogen werden,
		<!-- ohne das die nachfolgende Position(z.B.Fertigen) mit vorgezogen wird? -->
		<!-- true/default: Der ganze Vorgang(z.B.Rüsten + Fertigen) wird vorgezogen und zusammenhängend geplant -->
		<!-- false/Sonderfall: Die Rüstposition wird einzeln vorgezogen.Die nachfolgende Position(z.B.Fertigen) hält die Restriktionen zum vorherigen Vorgang ein -->
		<add key = ""BDE_VORGANG_KOMPLETT"" value=""true""/>
		
		<!-- Pro Vorgangsposition kann der über mehrere Meldungen verteilt gemeldete aktuelle Fortschritt mengenmäßig zusammengefasst werden -->
		<!-- 0: nur die Menge aus der aktuellsten Meldung gilt -->
		<!-- 1: Mengen aus allen gültigen BDE-Meldungen über alle Arbeitsplätze hinweg werden kumuliert -->
		<!-- 2: Mengen aus der jeweils neusten BDE-Meldung pro gemeldeten Arbeitsplatz werden kumuliert -->
		<add key = ""BDE_ACCUMULATE_PROGRESS"" value=""0""/>

		<!-- Modus für automatische Rückmeldung -->
		<!-- 1: Maximalen Fortschritt der Vorgänger annehmen(beendet, falls Nachfolger mind.gestartet)-->
		<!-- 2: Erwarteten Fortschritt der Vorgänger annehmen(mind.gestartet, falls Nachfolger mind.gestartet, bisherige Planung/Meldung berücksichtigen)-->
		<!-- 3: Minimalen Fortschritt der Vorgänger annehmen(mind.gestartet, falls Nachfolger mind.gestartet, bisherige Planung/Meldung nicht berücksichtigen)-->
		<add key = ""BDE_AUTOCONFIRM_TYPE"" value=""2""/>
		
		<!-- Innerhalb einer Produktionslinie dürfen sich Aufträge nicht überholen. -->
		<add key = ""SCHEDULE_ORDER_LINE"" value= ""false"" />


        < !--Bereits vorhandene Splits eines Vorgangs werden vor der Planung verworfen, falls ""automatisch splitten"" am Vorgang aktiviert -->
		<add key = ""OPT_SPLIT_RELEASE"" value= ""true"" />

        < !--Direkt aufeinanderfolgend geplante Splits eines Vorgangs werden zusammengeführt, falls ""automatisch splitten"" am Vorgang aktiviert -->
		<add key = ""OPT_SPLIT_MERGE"" value= ""true"" />

        < !--Soll der Wert für die minimale Splitanzahl beim Zusammenführen von Splits beachtet werden? -->
		<add key = ""OPT_SPLIT_MERGE_RESPECT_MIN_SPLIT"" value= ""true"" />


        < !--Grundzustand des Arbeitszeitmodells -->
		<!-- 0: Automatisch: Wenn Schichtmodell vorhanden, dann Grundzustand 'Pause', sonst 'verfügbar' -->
		<!-- 1: Grundzustand 'verfügbar' -->
		<!-- 2: Grundzustand 'Pause' -->
		<add key = ""WORKINGTIMEMODEL_INITIAL_ALLOCATION"" value= ""0"" />


        < !--Dürfen Schichtmodelle gemischt werden? -->
		<!-- true: Schichtmodell aus Gruppe, von welcher das Arbeitszeitmodell geerbt wird, dazu mischen. Schichtmodell der Einzelressource hat Priorität.Es gelten weiterhin die Gültigkeitszeiträume der Schichtmodelle, es werden keine Inhalte zusammengeführt. -->
		<!-- false: Schichtmodelle werden nicht gemischt. Entweder es gilt das Schichtmodell der Gruppe, falls Arbeitszeitmodell geerbt wird, oder das an der Einzelressource angegebene Schichtmodell -->
		<add key = ""WORKINGTIMEMODEL_MERGE_GROUP_SHIFTMODEL"" value= ""false"" />


        < !--Prioritätsboni: Wenn Bedingung erfüllt, wird die Priorität für die Verplanung des Mitarbeiters (maximal) um den angegebenen Wert erfüllt -->
		<!-- Faktor für Priorität aus Arbeitsplatzqualifikation -->
		<add key = ""OPT_WORKER_PRIO_WORKCENTERQUALIFICATION"" value= ""1.0"" />

        < !--Mitarbeiter bleibt am Arbeitsplatz -->
		<add key = ""OPT_WORKER_PRIO_KEEP_WORKCENTER"" value= ""0.25"" />

        < !--Mitarbeiter bleibt am Vorgang -->
		<add key = ""OPT_WORKER_PRIO_KEEP_OPERATION"" value= ""0.25"" />

        < !--Mitarbeiter bleibt von äquivalenten Vorgangsposition vom Vorgänger im Fertigungsauftrag -->
		<add key = ""OPT_WORKER_PRIO_KEEP_PREDECESSOR"" value= ""0.25"" />

        < !--Mitarbeiter bleibt von äquivalenten Vorgangsposition vom Vorgänger im Sekundär-Fertigungsauftrag -->
		<add key = ""OPT_WORKER_PRIO_KEEP_CHILD_ORDER"" value= ""0"" />

        < !--Maximaler Prioritätsbonus für geringere Personalkosten -->
		<add key = ""OPT_WORKER_PRIO_COSTS"" value= ""0"" />

        < !--Modus für die automatische Arbeitsplatzqualifikation des Personals -->
		<!-- 0: Keine automatische Qualifikation (Standard bis 5.6) -->
		<!-- 1: Wenn es für keinen Arbeisplatz qualifizierte Mitarbeiter gibt, dann sind automatisch alle Mitarbeiter für alle Arbeisplätze qualifiziert(Standard ab 5.7) -->
		<!-- 2: Wenn es für einen Arbeisplatz keine qualifizierte Mitarbeiter gibt, dann sind automatisch alle Mitarbeiter für diesen Arbeisplatz qualifiziert -->
		<add key = ""OPT_WORKER_AUTO_WORKCENTERQUALIFICATION_MODE"" value=""1"" />

		<!-- Dauer der automatischen Belegung von geplantem Personal ohne gegebene Belegungsintervalle(0: keine automatische Belegung, -1: komplette Dauer, sonst Dauer in Minuten) -->
		<add key = ""OPT_AUTO_WORKER_INTERVAL_DURATION"" value=""30"" />
		
		<!-- Maximale Abweichung(in Minuten) von Vorgabe für gleichmäßige manuelle Verschiebung(-1 = inaktiv) -->
		<add key = ""MANUAL_MOVE_MAXDIFFSTART"" value=""-1"" />
		<!-- Sicherstellen der Auftragsbeziehungen bei gleichmäßiger manueller Verschiebung -->
		<add key = ""MANUAL_MOVE_RESPECT_ORDER_RELATIONS"" value=""true"" />
		<!-- Sicherstellen der Vorgangsbeziehungen bei gleichmäßiger manueller Verschiebung -->
		<add key = ""MANUAL_MOVE_RESPECT_OPERATION_RELATIONS"" value=""true"" />
		
		<!-- Linienauswahl gilt über Nicht-Linienvorgänge hinweg -->
		<!-- default: false, Nach Unterbrechung durch einen Nicht-Linienvorgang kann eine neue Linie gewählt werden. -->
		<add key = ""OPT_KEEP_LINE_ACROSS_GAPS"" value=""false"" />
		<!-- Gültigkeit der Linienauswahl auf die Menge der Vorgänge mit gleichem Kennzeichen in der angegebenen Eigenschaft einschränken, default = """" -->
		<!-- Example: ""info3"" = Infofeld3 der FA-Vorgangsposition; ""productionorder.productionorder_id"" = Auftragsnummer -->
		<add key = ""OPT_KEEP_LINE_MARKER"" value="""" />
		<!-- Spezielle Rüstoptimierung für sequentielle Linien -->
		<add key = ""OPT_LINESEQ_SETUPOPT"" value=""false"" />
		<!-- Bei sequentiellen Linien die Vorgangszeiten synchronisieren(Dauer strecken und Überlappung anpassen) -->
		<add key = ""OPT_LINESEQ_SYNCHRONISE_TIMES"" value=""false"" />
		<!-- Bei sequentiellen Linien die Vorgangszeiten auch für Vorgänge mit Dauer 0 synchronisieren(Gilt nur, wenn OPT_LINESEQ_SYNCHRONISE_TIMES aktiviert ist.) -->
		<add key = ""OPT_LINESEQ_SYNCHRONISE_TIMES_NO_DURATION"" value=""true"" />

		<!-- Zusätzlicher Einplanungsaufwand, um günstigere Planung des MaxPuffer-Nachfolgers zu realisieren --->
		<!-- 0 = inaktiv, 1 = einfache Prüfung, > 1 = erweiterte Prüfung -->
		<add key = ""OPT_MAXBUFFER_SHIFTS"" value = ""0""/>

		<!-- Bei der Einplanung alternative Arbeitsplätze strikt bevorzugen, auf denen für den Vorgang keine Verspätungskosten anfallen -->
		<add key = ""OPT_PREFER_WORKCENTER_ON_TIME"" value=""false"" />
		<!-- Angefangene Verspätungskostenintervalle anteilig werten -->
		<add key = ""OPT_PRIORITY_LATENESSCO"""},
                new Config { PropertyName = "SelectedProductionControllerPermissions", Value = ""},
            };
            context.AddRange(configs);
            context.SaveChanges();

            var plannningparameter = new Planningparameter[]
            {
                new Planningparameter { PlanningparameterId = "Standard"}
            };
            context.AddRange(plannningparameter);
            context.SaveChanges();

            var modelparameter = new Modelparameter[]
            {
                new Modelparameter { ModelparameterId = "Standard", ActualTime = "20000101 00:00:00", ActualTimeFromSystemTime = 0 }
            };
            context.AddRange(modelparameter);
            context.SaveChanges();
        }
    }
}
