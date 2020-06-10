# <command>
#   <description>Startet ein externes Programm</description>
#   <param name="program" type="file">Pfad zum Programm</param>
#   <param name="arguments" type="string">(optional) Parameter für das Programm</param>
#   <param name="directorz" type="dir">(optional) Verzeichnis in dem das Programm ausgeführt wird</param>
# </command>
from System.Diagnostics import Process, ProcessStartInfo
setup = ProcessStartInfo(program, arguments)
if directory is not None:
    setup.WorkingDirectory = directory
Process.Start(setup)