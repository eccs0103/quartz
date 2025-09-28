variable1 Number: 10;
variable2 Number: 20;

if (variable1 > variable2) {
	write(variable1);
} else { 
	write(variable2);
}

if (variable1 > variable2) {
	write(variable1);
} else if (variable1 = variable2) {
	write(variable1, variable2);
} else {
	write(variable2);
}