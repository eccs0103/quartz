# Quartz

A simplified, custom-built programming language.

[Change log](./CHANGELOG.md) | [Grammar](./grammar.ebnf)

## Examples

Here are some concise examples demonstrating the core features of the Quartz language.

### 1. Hello, world!
The most basic program to print a message to the console.
```quartz
main() Null {
	write("Hello, world!");
}
```

### 2. Variables and types
Declaring variables of different types, including nullable types that can hold `null`.
```quartz
main() Null {
	// Basic types with initial values
	message String: "Hello from a variable";
	answer Number: 42;
	is_active Boolean: true;

	write(message);

	// Nullable type
	version Number?;
	write(version);  // null
	version : 1.1;
	write(version);  // 1.1
}
```

### 3. String concatenation
Combining strings to create new ones.
```quartz
main() Null {
	first_name String: "John";
	last_name String: "Doe";
	full_name String: first_name + " " + last_name;

	write("Full name: " + full_name);
}
```

### 4. Control flow
Executing code based on conditions.
```quartz
main() Null {
	temperature Number: 25;
	if (temperature > 30) {
		write("It's a hot day!");
	} else if (temperature > 20) {
		write("It's a pleasant day.");
	} else {
		write("It's a cool day.");
	}
}
```

### 5. Loops
Repeating a block of code and iterating over a sequence.
```quartz
main() Null {
	// While loop
	counter Number: 1;
	while (counter <= 5) {
		write(counter.to_string());
		counter : counter + 1;
	}

	// For loop
	sum Number: 0;
	for (i Number in range(1, 11)) {
		sum : sum + i;
	}
	write("Sum of 1 to 10 is: " + sum.to_string()); // 55
}
```

### 6. Functions & return values
Defining reusable blocks of code. Functions can take parameters and return values.
```quartz
// Example of a function returning a value
add(a Number, b Number) Number {
	return a + b;
}

// Example of a null function
greet(name String) Null {
	write("Hello, " + name + "!");
}

main() Null {
	greet("Alice");
	result Number: add(5, 3);
	write("5 + 3 = " + result.to_string()); // 8
}
```

### 7. Arrays
Working with ordered collections of data.
```quartz
main() Null {
	// Create an array with initial values
	numbers Array<Number>: [10, 20, 30, 40, 50];

	// Access an element by its zero-based index
	write("Element at index 2: " + numbers[2].to_string()); // 30

	// Modify an element
	numbers[2] : 35;
	write("New element at index 2: " + numbers[2].to_string()); // 35
}
```

### 8. FizzBuzz
A classic programming task demonstrating loops, conditionals, and a custom function to calculate remainders, as Quartz does not have a built-in modulo operator.
```quartz
// Quartz uses integer division. We can create a remainder function.
remainder(a Number, b Number) Number {
	return a - (a / b) * b;
}

main() Null {
	for (i Number in range(1, 16)) {
		is_fizz Boolean: remainder(i, 3) = 0;
		is_buzz Boolean: remainder(i, 5) = 0;

		if (is_fizz & is_buzz) {
			write("FizzBuzz");
		} else if (is_fizz) {
			write("Fizz");
		} else if (is_buzz) {
			write("Buzz");
		} else {
			write(i.to_string());
		}
	}
}
```
