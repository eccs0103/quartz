## Quartz

Custom simplified programming language model.

[Change log](./CHANGELOG.md), [Grammar](./Grammar.ebnf)

## Basic Types and Variables

#### Variables with initialization
```
temperature Number(36.6);
isActive Boolean(true);
name String("Alice");
```

#### Optional variables (nullable)
```
optional Number?;
write(optional);  // null
optional : 42;
write(optional);  // 42
```

#### Polymorphic Any type
```
value Any(100);
write(value);     // 100

value : "text";
write(value);     // text

value : true;
write(value);     // true
```

## Arithmetic Operations

#### Basic math
```
celsius Number(36.6);
fahrenheit Number(32 + celsius * 1.8);
write(fahrenheit);
```

#### Operator precedence
```
result Number(2 + 3 * 4);
write(result);  // 14

result : (2 + 3) * 4;
write(result);  // 20
```

## String Operations

#### String concatenation
```
firstName String("John");
lastName String("Doe");
fullName String(firstName + " " + lastName);
write(fullName);  // John Doe

greeting String("Hello, " + fullName + "!");
write(greeting);
```

## Character Type

#### Character literals
```
first Character('a');
last Character('z');
write(first);   // a
write(last);    // z
```

#### Character operations
```
letter Character('a');
write(letter.to_string());  // a
write(letter.to_number());  // 97

upper Character('Z');
lower Character('z');
write(upper < lower);   // true
write(upper > lower);   // false
```

#### Character and string concatenation
```
initial Character('J');
rest String("ohn");
full String(initial + rest);
write(full);  // John

prefix String("Hello, ");
excl Character('!');
write(prefix + excl);  // Hello, !
```

## Boolean Logic

#### Logical operators
```
a Boolean(true);
b Boolean(false);

write(a & b);   // false (AND)
write(a | b);   // true (OR)
write(!a);      // false (NOT)
```

#### Comparisons
```
x Number(10);
y Number(20);

write(x < y);   // true
write(x > y);   // false
write(x = 10);  // true
write(x != y);  // true
write(x <= 10); // true
write(y >= 20); // true
```

## Control Flow

#### If-else statements
```
score Number(85);
passed Boolean;

if (score >= 60) {
	passed : true;
	write("Passed!");
} else {
	passed : false;
	write("Failed!");
}
```

#### Nested conditions
```
grade Number(85);
letter String;

if (grade >= 90) {
	letter : "A";
} else if (grade >= 80) {
	letter : "B";
} else if (grade >= 70) {
	letter : "C";
} else {
	letter : "F";
}
write("Grade: " + letter);
```

## Loops

#### While loop with counter
```
counter Number(1);
while (counter <= 5) {
	write(counter);
	counter : counter + 1;
}
```

#### Break statement
```
i Number(1);
while (i <= 10) {
	write(i);
	if (i = 5) {
		break;
	}
	i : i + 1;
}
write("Loop ended");
```

#### Continue statement
```
i Number(0);
while (i < 5) {
	i : i + 1;
	if (i = 3) {
		continue;
	}
	write(i);  // prints 1, 2, 4, 5 (skips 3)
}
```

#### For loop
```
for (i Number in range(5)) {
	write(i);  // prints 0, 1, 2, 3, 4
}
```

#### For loop with custom range
```
for (n Number in range(3, 8)) {
	write(n);  // prints 3, 4, 5, 6, 7
}
```

## Practical Examples

#### Calculate factorial
```
n Number(5);
result Number(1);
i Number(1);

while (i <= n) {
	result : result * i;
	i : i + 1;
}
write("Factorial: ");
write(result);  // 120
```

#### Fibonacci sequence
```
limit Number(10);
prev Number(0);
curr Number(1);
count Number(0);

while (count < limit) {
	write(prev);
	temp Number(prev + curr);
	prev : curr;
	curr : temp;
	count : count + 1;
}
```

#### Find maximum of three numbers
```
a Number(15);
b Number(42);
c Number(28);
max Number;

if (a > b) {
	if (a > c) {
		max : a;
	} else {
		max : c;
	}
} else {
	if (b > c) {
		max : b;
	} else {
		max : c;
	}
}
write("Maximum is: ");
write(max);
```

#### Temperature converter
```
celsius Number(25);
choice String("F");

if (choice = "F") {
	fahrenheit Number(celsius * 1.8 + 32);
	write("Temperature in Fahrenheit: ");
	write(fahrenheit);
} else {
	write("Temperature in Celsius: ");
	write(celsius);
}
```

#### Simple calculator
```
a Number(10);
b Number(5);
operation String("+");
result Number;

if (operation = "+") {
	result : a + b;
} else if (operation = "-") {
	result : a - b;
} else if (operation = "*") {
	result : a * b;
} else if (operation = "/") {
	result : a / b;
}
write("Result: ");
write(result);
```

#### Count even and odd numbers
```
start Number(1);
end Number(10);
even Number(0);
odd Number(0);
current Number(start);

while (current <= end) {
	remainder Number(current - (current / 2) * 2);
	if (remainder = 0) {
		even : even + 1;
	} else {
		odd : odd + 1;
	}
	current : current + 1;
}
write("Even: ");
write(even);
write("Odd: ");
write(odd);
```

## Advanced Features

#### Generic types
```
optionalNumber Nullable<Number>(42);
write(optionalNumber);

optionalString Nullable<String>("Hello");
write(optionalString);

optionalBool Nullable<Boolean>;
write(optionalBool);  // null
```

#### Type system
```
write(Type);
write(Number);
write(Boolean);
write(String);
write(Any);
write(Nullable);
```

#### Constants
```
radius Number(5);
area Number(pi * radius * radius);
write("Area of circle: ");
write(area);

exponent Number(e * e);
write("e squared: ");
write(exponent);
```

#### Scopes and blocks
```
outer Number(10);
{
	inner Number(20);
	write(outer);  // 10
	write(inner);  // 20
	outer : 15;
}
write(outer);  // 15
```

#### Member access and method calls
```
n Number(42);
label String("Value: " + n.to_string());
write(label);  // Value: 42

ch Character('A');
write(ch.to_number());  // 65

b Boolean(true);
result String("Is active: " + b.to_string());
write(result);  // Is active: true
```

#### Sequences and ranges
```
nums Sequence<Number>(range(1, 4));
while (nums.next()) {
	current Number(nums.current());
	write(current);  // prints 1, 2, 3
}
```
