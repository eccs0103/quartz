swap(mutable array Array<Number>, index_1 Integer, index_2 Integer) Null {
	temporary Number(array[index_1]);
	array[index_1]: array[index_2];
	array[index_2]: temporary;
}

bubble_sort(mutable array Array<Number>) Null {
	for (index_1 Integer in range(0, array.length)) {
		for (index_2 Integer in range(index_1, array.length)) {
			swap(array, index_1, index_2);
		}
	}
}

array Array<Integer>(8, 15, 32, 4, 0, -5);
write(array);
bubble_sort(array);
write(array);
