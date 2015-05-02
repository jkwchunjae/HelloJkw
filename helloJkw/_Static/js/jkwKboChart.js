
$('select#seasonSelect').change(function () {
	location.href = '/kbochart/' + this.value;
});

$('div.datepicker').datepicker({
	format: 'yyyy-mm-dd',
	todayBtn: 'linked',
	autoclose: true
})
.on('changeDate', function (e) {
	var date = $('div.datepicker').datepicker('getDate');
	var year = date.getFullYear();
	var date8 = year + padStr(date.getMonth() + 1) + padStr(date.getDate());
	$('div.datepicker').css('display', 'none');

	updateStanding(year, date8);
});

$('button#prev-date').click(function () {
	var date = $('div.datepicker').datepicker('getDate');
	var prevDate = new Date(date);
	prevDate.setDate(date.getDate() - 1);
	$('div.datepicker').datepicker('setDate', prevDate);
});

$('button#next-date').click(function () {
	var date = $('div.datepicker').datepicker('getDate');
	var prevDate = new Date(date);
	prevDate.setDate(date.getDate() + 1);
	$('div.datepicker').datepicker('setDate', prevDate);
});

$('button#today-date').click(function () {
	var date = new Date();
	$('div.datepicker').datepicker('setDate', date);
});

function padStr(value) {
	return (value < 10) ? '0' + value : '' + value;
}

$('select#select-diff-rank').change(function () {
	//$('.diff-rank').css('display', 'none');
	//$('.diff-rank-' + $(this).val()).css('display', 'table-cell');
	$('.diff-rank-' + $(this).val()).each(function () {
		$(this).parent() // tr
			.children('td#diff-rank-innercell')
			.html($(this).html());
	});
});

function makeDiffRankElement(diffRank) {
	var spanRank = document.createElement('span');

	$(spanRank).text(Math.abs(diffRank)).addClass('diff-rank-rank');
	if (diffRank == 0) {
		$(spanRank).text('-');
	} else if (diffRank < 0) {
		$(spanRank).addClass('rank-arrow').addClass('rank-arrow-up');
	} else if (diffRank > 0) {
		$(spanRank).addClass('rank-arrow').addClass('rank-arrow-down');
	}

	return spanRank;
}

function updateStanding(year, date) {
	$.post('/kbochart/standing/' + year + '/' + date, function (data) {
		var result = eval('(' + data + ')');
		var standingList = result.standing;
		var tbodyStandingList = $('tbody.standing-list');
		tbodyStandingList.empty();
		standingList.forEach(function (standing, index, array) {
			var tr = document.createElement('tr');
			$(tr).append($(document.createElement('td')).text(standing.Rank).addClass('align-center'));
			$(tr).append($(document.createElement('td')).append(makeDiffRankElement(standing.Diff1y)).attr('id', 'diff-rank-innercell'));
			$(tr).append($(document.createElement('td')).append(makeDiffRankElement(standing.Diff1d)).addClass('diff-rank').addClass('diff-rank-1d'));
			$(tr).append($(document.createElement('td')).append(makeDiffRankElement(standing.Diff3d)).addClass('diff-rank').addClass('diff-rank-3d'));
			$(tr).append($(document.createElement('td')).append(makeDiffRankElement(standing.Diff7d)).addClass('diff-rank').addClass('diff-rank-7d'));
			$(tr).append($(document.createElement('td')).append(makeDiffRankElement(standing.Diff2w)).addClass('diff-rank').addClass('diff-rank-2w'));
			$(tr).append($(document.createElement('td')).append(makeDiffRankElement(standing.Diff1m)).addClass('diff-rank').addClass('diff-rank-1m'));
			$(tr).append($(document.createElement('td')).append(makeDiffRankElement(standing.Diff2m)).addClass('diff-rank').addClass('diff-rank-2m'));
			$(tr).append($(document.createElement('td')).append(makeDiffRankElement(standing.Diff1y)).addClass('diff-rank').addClass('diff-rank-1y'));
			$(tr).append($(document.createElement('td')).append(makeDiffRankElement(standing.Diff2y)).addClass('diff-rank').addClass('diff-rank-2y'));
			$(tr).append($(document.createElement('td')).text(standing.Team).addClass('align-center'));
			$(tr).append($(document.createElement('td')).text(standing.Game).addClass('align-right'));
			$(tr).append($(document.createElement('td')).text(standing.Win).addClass('align-right'));
			$(tr).append($(document.createElement('td')).text(standing.Draw).addClass('align-right'));
			$(tr).append($(document.createElement('td')).text(standing.Lose).addClass('align-right'));
			$(tr).append($(document.createElement('td')).text(standing.PCT).addClass('align-right'));
			$(tr).append($(document.createElement('td')).text(standing.GB).addClass('align-right'));
			$(tr).append($(document.createElement('td')).text(standing.Last10).addClass('align-center').addClass('standing-pc'));
			$(tr).append($(document.createElement('td')).text(standing.STRK).addClass('align-center').addClass('standing-pc'));
			//$(tr).append($(document.createElement('td')).text(standing.Home).addClass('standing-pc'));
			//$(tr).append($(document.createElement('td')).text(standing.Away).addClass('standing-pc'));
			$(tbodyStandingList).append(tr);
		});

		$('span#last-update-time').text(result.updateTime);

		$('h3').text('순위표 (' + date + ')');
	});
}

$('button#pick-date').click(function () {
	var datepicker = $('div.datepicker');
	if ($(datepicker).css('display') != 'none') {
		$(datepicker).css('display', 'none');
	}
	else {
		$(datepicker).css('display', 'block');
	}
});

$('div.datepicker').datepicker('setDate', '@Model.LastDate');