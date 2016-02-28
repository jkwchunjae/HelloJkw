function getDate8(date /* moment object */) {
	return date.year() + '.' + padStr(date.month() + 1) + '.' + padStr(date.date());
}

function padStr(value) {
	return (value < 10) ? '0' + value : '' + value;
}