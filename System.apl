Vector2 {
	_x Number;
	_y Number;

	@getter
	x() Number {
		return this._x;
	};

	@setter
	x(value Number) {
		this._x = value;
	};

	to_string() String {
		return "";
	}
}





use { Iterator, Console } from "system";
use { range } from Iterator;
use { write } from Console;

Array<T> {
	swap(index_1 Integer, index_2 Integer) Null {
		temporary T: this[index_1];
		this[index_1]: this[index_2];
		this[index_2]: temporary;
	}
}

Array<Number> {
	bubble_sort() Null {
		for (index_1 Integer in range(0, this.length)) {
			for (index_2 Integer in range(index_1, this.length)) {
				this.swap(index_1, index_2);
			}
		}
	}
}

Developer {
	main() Null {
		array Array<Integer>: [8, 15, 32, 4, 0, -5];
		write(array);
		array.bubble_sort();
		write(array);
	}
}
