function getDate8(date /* moment object */) {
	try {
		return date.year() * 10000 + (date.month() + 1) * 100 + date.date();
	}
	catch (err) {
		return 0;
	}

}

function getDateDot(date /* moment object */) {
	return date.year() + '.' + padStr(date.month() + 1) + '.' + padStr(date.date());
}

function padStr(value) {
	return (value < 10) ? '0' + value : '' + value;
}