function getDate8(date) {
	return date.getFullYear() + '.' + padStr(date.getMonth() + 1) + '.' + padStr(date.getDate());
}

function padStr(value) {
	return (value < 10) ? '0' + value : '' + value;
}