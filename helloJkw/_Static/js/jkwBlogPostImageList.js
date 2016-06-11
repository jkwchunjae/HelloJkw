
$(document).ready(function () {

	function initImageList() {
		var imageListIndex = 0;
		$('div.image-list').each(function () {
			$(this).attr('data-index', imageListIndex)
			imageListIndex++

			var imageIndex = 0;
			$(this).find('img').each(function () {
				$(this).attr('data-index', imageIndex)
				imageIndex++
			})

			$(this).find('img[data-index="0"]').css('display', 'block')
		})
	}

	initImageList()

	$('.image-list img').click(function () {
		var imageCount = $(this).parent().find('img').length
		var index = parseInt($(this).attr('data-index'), 10)

		$(this).css('display', 'none')
		$('.image-list img[data-index="' + ((index + 1) % imageCount) + '"]').css('display', 'block')
	})
});