# ModbusLib library notes

The ModbusLib class library relies on a subset of some implementation materials from https://github.com/highfield/cetdevelop

## Notice from initial fork 2024

Original code of the lib has not been modified but a lot of work should be needed, especially in the following area
- thread safety
- error management
- connection recovery

Those topics are show-stoppers regarding performance and/or robustness requirements. 

The 1th purpose of the fork was just to enhance the usability of the master/scanner and the device slave. 
Usage of the tools should be limited to testing domain, in particular with single endpoint communications.
Indeed, the scanner is single-target oriented, which restricts applicability.

