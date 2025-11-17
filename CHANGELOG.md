## 0.4.3 (17.11.2025)
- Semicolons (`;`) at the end of blocks are now optional.
- Allowed the use of extra (superfluous) semicolons.
- Added support for single-statement blocks without braces (`{`, `}`).

## 0.4.2 (14.11.2025)
- Improved OOP structure.
- Added the ability to add primitive blocks.
- Improved the assignment system.
- Added `Null` type.

## 0.4.0 (14.11.2025)
- Added scopes.
- Added `if`, `else` operators and their operation with `Boolean` data.
- Added comparison operators `=`, `<`, `<=`, `>`, `>=` for `Number` data.
- Improved and updated the language grammar. Improved variable declarations.
- Added a simplified OOP system.

## 0.3.0 (24.09.2025)
- Introduced strict typing and operators working with them into the core.
- Improved error descriptions and ranges.
- Strictly defined grammar.
- Fixed numerous minor bugs.

## 0.1.16 (13.08.2024)
- Added strings and the `import` keyword. Currently, strings are used only to specify the path after the `import` keyword. The `import` keyword allows you to execute code from a file at the specified path using the **current** interpreter.
- Added the ability to specify paths as interpreter launch arguments, thereby importing other scripts immediately upon execution.

## 0.1.15 (03.08.2024)
- Added the future to call functions. Currently, there is only the `Write(...Values)` function instead of the `print` keyword.
- Optimized code execution in the following areas:
  - token parsing,
  - keyword recognition,
  - token sequence traversal,
  - creation of regions in brackets,
  - tree parsing for various operations,
  - evaluation of the tree from different nodes.
- Fixed token position detection.
- Any error now includes its position.
- Fixed parsing errors with multiple brackets.

## 0.1.12 (10.02.2024)
- Temporarily removed unstable operators `+:`, `-:`, `*:`, `/:`.
- Interpreter split into parts for easier management. Code simplified using patterns.
- Parser structure changed. Pointer tracking during code reading implemented. Parser errors now indicate error location.
- Keyword `print` now requires parentheses.

## 0.1.10 (04.12.2023)
- Added operators `+:`, `-:`, `*:`, `/:`.
- Optimized token parsing.

## 0.1.9 (29.11.2023)
- Added the keyword `null`. To use a missing value, it's necessary to explicitly use `null`. The absence of a value is no longer automatically considered as `null`.
- Improved recognition of semicolons.
- Now the initial variables `E` and `Pi` are considered non-writable. Their values cannot be changed.
- Improved adaptability and interpretation of variables.
- Fixed a bug where it was possible to initialize a variable with itself.


## 0.1.7 (28.11.2023)
- Now you can execute code with multiple instructions.
- Added the ability to work with signed numbers. For example: `+2`, `-4`.
- Improved language adaptability. Now, the absence of a value anywhere will be interpreted as `null`;
- Changed syntax. Now it's mandatory to put a semicolon after each instruction.

## 0.1.5 (27.11.2023)
- Optimized parsing of code in brackets.
- Improved error descriptions.
- Added the ability to put a semicolon at the end of a line. It can also be omitted.
- Added the ability to declare variables, initialize them, and change their values.
- Now, only data in the `print()` key block will be output to the console.

## Release 0.1.2 (24.11.2023)
First stable version