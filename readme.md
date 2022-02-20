# aplibsharp
aplibsharp is an open source version of [Jørgen Ibsen's](https://github.com/jibsen) [aPLib](http://ibsensoftware.com/products_aPLib.html) compression library.  
The goal of aplibsharp is to approach[*](#notes) maximum compression ratio within the constraints of the aPLib format.  
## Directory structure
|Directory| Description|
|-|-|
|aplib |the compression library|
|appack|console application|
|cap|c64 specific console application|
|depackers|depackers for various architectures|
### appack usage
Compress file:
```sh
appack e infile outfile
```
Decompress file:
```sh
appack d infile outfile
```
### cap usage
Compress file:
```sh
cap e infile outfile
```
Decompress file:
```sh
cap d infile outfile
```
Compress executable (\$0200&ndash;\$FFF?)[**](#notes)
```sh
cap x --s=64738 --c=55 --i=1 inprg outprg
```
Flags:  
`-s` - jump address  
`-c` - cpu port register  
`-i` - interrupt flag  
Flags supports decimal and `0x`-prefixed hexadecimal values.  
### License
`aplibsharp`, `appack` and `cap` are released under the MIT license.  
Depackers are Copyright &copy; of the respective authors
### Credits
[Suffix Array](https://sites.google.com/site/yuta256/sais) by [Yuta Mori](https://github.com/y-256)  
z80 depacker by [Antonio José Villena Godoy](https://github.com/antoniovillena)  
### Notes
(*) Two constants needs to be edited in `aplib/constant.cs` to actually reach optimal compression ratio. By default these are set to shorten encoding time. File size increase is usually about a pcm due to this.  
(**) Care need to be taken when compressing c64 files near top of memory. Incompressible data might cause source and destination to intersect during decompression.  
This library has been tested quite extensively. Please contact me should you find a file that doesn't compress correctly using this library.  
