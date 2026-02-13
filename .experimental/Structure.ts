//#region Any
export interface Any {
}
//#endregion

//#region Nullable
export interface Nullable<T> extends Any {
}

export interface PrototypeNullable {
	new <T>(value: T): Nullable<T>;
	readonly null: unique symbol;
}

export declare var Nullable: PrototypeNullable;

export var $null = Nullable.null;
//#endregion

//#region Object
export interface Object extends Any {
	to_string(): String;
}
//#endregion

//#region Boolean
export interface Boolean extends Object {
}

export interface PrototypeBoolean {
	new(value: Boolean): Boolean;
	readonly true: Boolean;
	readonly false: Boolean;
}

export declare var Boolean: PrototypeBoolean;

export var $true = Boolean.true;
export var $false = Boolean.false;
//#endregion

//#region Number
export interface Number extends Object {
}

export interface PrototypeNumber {
	new(value: Number): Number;
}

export declare var Number: PrototypeNumber;
//#endregion

//#region Integer
export interface Integer extends Number {
}

export interface PrototypeInteger extends PrototypeNumber {
	new(value: Integer): Integer;
}

export declare var Integer: PrototypeInteger;
//#endregion

//#region Index
export interface Index extends Integer {
}

export interface PrototypeIndex extends PrototypeInteger {
	new(value: Index): Index;
}

export declare var Index: PrototypeIndex;
//#endregion

//#region Character
export interface Character extends Object {
}

export interface PrototypeCharacter {
	new(value: Character): Character;
}

export declare var Character: PrototypeCharacter;
//#endregion

//#region Sequence
export interface Sequence<T> extends Object {
	get value(): T;
	get next(): Sequence<T>;
}
//#endregion

//#region Array
export interface Array<T> extends Sequence<T> {
}

export interface PrototypeArray {
	new <T>(value: Sequence<T>): Array<T>;
}

export declare var Array: PrototypeArray;
//#endregion

//#region String
export interface String extends Array<Character> {
}

export interface PrototypeString {
	new(value: Sequence<Character>): String;
}

export declare var String: PrototypeString;
//#endregion

//#region Type
export interface Type extends Any {
}

export interface PrototypeType {
	new(value: Type): Type;
}

export declare var Type: PrototypeType;
//#endregion
