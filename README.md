## Programming Language

Custom simplified programming language model.

[Change log](./CHANGELOG.md), [Grammar](./Grammar.ebnf)

#### Variables
```
var1 Number;
var2 Number(3);
var3 Boolean(true);
var4 String("string value");
```

#### Simple math
```
celsius Number(36.6);
fahrenheit Number(32 + celsius * 1.8);
write(fahrenheit);
```

#### Typed core
```
write(Type);
write(Function);
write(Number);
write(Boolean);
write(String);
```

#### If-else statements
```
arg1 Number(10);
arg2 Number(20);
max Number;
indication Number;

min Number(arg1);
if (arg2 < arg1) {
	min: arg2;
};
write(min);

if (arg1 > arg2) {
	max: arg1;
} else { 
	max: arg2;
};
write(max);

if (arg1 > arg2) {
	indication: -1;
} else if (arg1 = arg2) {
	indication: 0;
} else {
	indication: 1;
};
write(indication);
```
