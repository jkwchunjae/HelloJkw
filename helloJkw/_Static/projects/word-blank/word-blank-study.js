$(document).on('click', 'a.split-str', function () {
	$.post('/wb/word-blank', {
		name: $(this).attr('data-name'),
		index: $(this).attr('data-index'),
		length: $(this).attr('data-length')
	}, function (data) {
		if (data == 'success') {
			loadTextList(pathList)
		} else {
			alert(data)
		}
	});
})
