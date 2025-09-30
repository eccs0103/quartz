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
