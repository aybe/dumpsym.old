# dumpsym
SN-Systems' DUMPSYM + batch renaming for Ghidra and IDA

## Features
- complete port of SN-Systems' DUMPSYM utility to the C# language
  - the bug that would output an extra address without a symbol at EOF has been fixed
  - it can be used as a library as well for extracting the list of symbols from a .SYM file
- generate Python scripts for both Ghidra and IDA for batch renaming symbols of type 2

## Usage for Ghidra
- copy generated script to `$USER_HOME/ghidra_scripts`
- open _Window, Script Manager_
- click _Manage Script Directories_
- check `$USER_HOME/ghidra_scripts`, close window
- find the script in _PSX_ category, double-click it

## Usage for IDA

- open _File, Script command..._
- change *Scripting language* to _Python_
- paste generated script, click _Run_
- to fix tail byte renaming errors, you can do the following:
  - double-click the address in the console
  - in *IDA View*, right-click the address
  - click _Array..._
  - uncheck _Create as array_, click _OK_
  - run the script again
  - rinse and repeat

## Links

https://github.com/lab313ru/dumpsym_src

https://github.com/lab313ru/ghidra_psx_ldr
