$(document).ready(function () {
})

$(document).on('keyup', 'textarea', function () {
	$(this).css('height', 'auto');
	$(this).height(this.scrollHeight);
})

$(document).on({
	mouseenter: function () {
		$(this).children('.button-list').css('visibility', 'visible');
	},
	mouseleave: function () {
		$(this).children('.button-list').css('visibility', 'hidden');
	}
}, '.member .member-info');

$('.datetimepicker').datetimepicker({
	useStrict: true,
	format: 'YYYY.MM.DD'
}).on('dp.change', function (e) {
	$(this).attr('data-date', getDate8(e.date));
});

$(document).on('keydown', '.datetimepicker input', function () {
	return false;
})
/*
$('.datetimepicker input').keydown(function () {
	return false;
});
*/

function changeMemberType(memberName, newMemberType) {
	$.post('fnb/member/changeType', {
		memberName: memberName,
		newMemberType: newMemberType
	}, function (data) {
		if (data == 'success') {
			loadMemberRegular()
			loadMemberAssociate()
		} else {
			showAlert(data)
		}
	})
}

$(document).on('click', 'button.btn-member-goto-regular', function () {
	var memberName = $(this).attr('data-member-name')
	changeMemberType(memberName, 'Regular')
})

$(document).on('click', 'button.btn-member-goto-associate', function () {
	var memberName = $(this).attr('data-member-name')
	changeMemberType(memberName, 'Associate')
})

$(document).on('click', 'button.btn-member-edit', function () {
	var memberName = $(this).attr('data-member-name')
	var joinDateDot = $(this).attr('data-member-joindate')
	$('#datetimepicker-' + memberName).datetimepicker({
		useStrict: true,
		format: 'YYYY.MM.DD'
	}).on('dp.change', function (e) {
		$(this).attr('data-date', getDate8(e.date))
	})

	// defaultDate 잘 안되서 이렇게라도..
	$('#datetimepicker-' + memberName + ' input').val(joinDateDot)
	$('#datetimepicker-' + memberName).attr('data-date', getDate8(moment(joinDateDot, 'YYYY.MM.DD')))

	$('#member-info-' + memberName).css('display', 'none')
	$('#member-edit-' + memberName).css('display', 'block')
})

$(document).on('click', 'button.btn-member-edit-cancel', function () {
	var memberName = $(this).attr('data-member-name')
	$('#member-info-' + memberName).css('display', 'block')
	$('#member-edit-' + memberName).css('display', 'none')
})

$(document).on('click', 'button.btn-member-edit-save', function () {
	var memberName = $(this).attr('data-member-name')
	var role = $('#member-role-' + memberName).val()
	var team = $('#member-team-' + memberName).val()
	var joinDate = $('#datetimepicker-' + memberName).attr('data-date')

	$.post('fnb/member/edit', {
		name: memberName,
		role: role,
		team: team,
		joinDate: joinDate
	}, function (data) {
		if (data == 'success') {
			loadMemberRegular()
			loadMemberAssociate()
		} else {
			showAlert(data)
		}
	})
})

$(document).on('click', 'button.btn-member-leave', function () {
	showAlert('더블클릭!')
})

$(document).on('dblclick', 'button.btn-member-leave', function () {
	var memberName = $(this).attr('data-member-name')
	$.post('fnb/member/leave', {
		memberName: memberName
	}, function (data) {
		if (data == 'success') {
			loadMemberRegular()
			loadMemberAssociate()
		} else {
			showAlert(data)
		}
	})
})

function clearMemberAdd(memberType) {
	$('#member-name-' + memberType).val('')
	$('#member-role-' + memberType).val('')
	$('#member-team-' + memberType).val('')
	$('#datetimepicker-' + memberType).data('DateTimePicker').clear();
}

$('.btn-member-add').click(function () {
	var memberType = $(this).attr('data-member-type')
	var memberData = {
		name: $('#member-name-' + memberType).val(),
		role: $('#member-role-' + memberType).val(),
		team: $('#member-team-' + memberType).val(),
		joinDate: $('#datetimepicker-' + memberType).attr('data-date'),
		memberType: memberType
	}

	$.post('fnb/member/add', memberData, function (data) {
		if (data == 'success') {
			loadMemberRegular()
			loadMemberAssociate()
			clearMemberAdd(memberType)
		} else {
			showAlert(data)
		}
	})
})

$('.btn-member-add-clear').click(function () {
	var memberType = $(this).attr('data-member-type')
	clearMemberAdd(memberType)
})

$(document).on('click', 'button.add-meeting-member-button', function () {
	if ($(this).hasClass('active')) {
		$(this).removeClass('btn-default').addClass('btn-success')
	} else {
		$(this).removeClass('btn-success').addClass('btn-default')
	}
})

$('button#button-add-meeting').click(function () {
	var attendantsArray = new Array();
	$('tr#tr-add-meeting button.add-meeting-member-button').each(function () {
		if ($(this).hasClass('active')) {
			var name = $(this).attr('data-member-name');
			attendantsArray.push(name);
		}
	})

	$.post('fnb/meeting/add', {
		no: $('#add-meeting-no').val(),
		date: $('#datetimepicker-meeting').attr('data-date'),
		attendants: attendantsArray.join(),
		others: $('#add-meeting-others').val()
	}, function (data) {
		if (data == 'success') {
			loadMeeting();
			loadMemberRegular();
			loadMemberAssociate();
			$('#add-meeting-no').val('');
			$('#datetimepicker-meeting').data('DateTimePicker').clear();
			$('#add-meeting-others').val('');
		} else {
			showAlert(data);
		}
	});
});

$(document).on('click', 'button.button-meeting-edit', function () {
	var tr = $(this).parent().parent().parent();
	var meetingNo = $(tr).attr('data-meeting-no');

	// 날짜
	var dateDot = $(tr).find('.meeting-date .meeting-info').text()
	$(tr).find('.datetimepicker').datetimepicker({
		useStrict: true,
		format: 'YYYY.MM.DD'
	}).on('dp.change', function (e) {
		$(this).attr('data-date', getDate8(e.date))
	})
	$(tr).find('.datetimepicker input').val(dateDot)
	$(tr).find('.datetimepicker').attr('data-date', getDate8(moment(dateDot, 'YYYY.MM.DD')))

	// 참석자
	var isAttandant = function (name) {
		var meeting = getMeeting(meetingNo)
		for (var index in meeting.Attendants) {
			var attendantName = meeting.Attendants[index]
			if (attendantName == name)
				return true
		}
		return false
	}
	$(tr).find('button.add-meeting-member-button').each(function () {
		var attendantName = $(this).text()
		if (isAttandant(attendantName)) {
			$(this).addClass('active').addClass('btn-success')
		}
	})

	// 비고
	var textOthers = $(tr).find('.meeting-edit textarea');
	$(textOthers).val($(textOthers).attr('data-others'))

	$(tr).find('.meeting-info').css('display', 'none')
	$(tr).find('.meeting-edit').css('display', 'block')
})

$(document).on('click', 'button.button-meeting-edit-save', function () {
	var tr = $(this).parent().parent().parent();
	var meetingNo = $(tr).attr('data-meeting-no');

	var attendantsArray = new Array();
	$(tr).find('button.add-meeting-member-button').each(function () {
		if ($(this).hasClass('active')) {
			var name = $(this).attr('data-member-name');
			attendantsArray.push(name);
		}
	})

	$.post('fnb/meeting/edit', {
		no: meetingNo,
		date: $(tr).find('.datetimepicker').attr('data-date'),
		attendants: attendantsArray.join(),
		others: $(tr).find('.meeting-edit textarea').val()
	}, function (data) {
		if (data == 'success') {
			loadMeeting();
			loadMemberRegular();
			loadMemberAssociate();
		} else {
			showAlert(data);
		}
	});
})

$(document).on('click', 'button.button-meeting-edit-cancel', function () {
	var tr = $(this).parent().parent().parent();
	$(tr).find('.meeting-info').css('display', 'block')
	$(tr).find('.meeting-edit').css('display', 'none')
})

$(document).on('dblclick', 'button.button-meeting-delete', function () {
	$.post('fnb/meeting/delete', {
		no: $(this).attr('data-meeting-no')
	}, function (data) {
		if (data == 'success') {
			loadMeeting()
		} else {
			showAlert(data)
		}
	})
})

$('button#button-accounting-add').click(function () {
	$.post('fnb/accounting/add', {
		date: $('#datetimepicker-accounting').attr('data-date'),
		content: $('#text-accounting-content').val(),
		outMoney: $('#text-accounting-outMoney').val(),
		inMoney: $('#text-accounting-inMoney').val()
	}, function (data) {
		if (data == 'success') {
			loadAccountingData()
			$('#datetimepicker-accounting').data('DateTimePicker').clear()
			$('#text-accounting-content').val('')
			$('#text-accounting-outMoney').val('')
			$('#text-accounting-inMoney').val('')
		} else {
			showAlert(data)
		}
	})
})

$(document).on('click', 'button.button-accounting-edit', function () {
	var tr = $(this).parent().parent().parent();

	// 날짜
	var dateDot = $(tr).find('.accounting-date .accounting-info').text()
	$(tr).find('.datetimepicker').datetimepicker({
		useStrict: true,
		format: 'YYYY.MM.DD'
	}).on('dp.change', function (e) {
		$(this).attr('data-date', getDate8(e.date))
	})
	$(tr).find('.datetimepicker input').val(dateDot)
	$(tr).find('.datetimepicker').attr('data-date', getDate8(moment(dateDot, 'YYYY.MM.DD')))

	// 내용
	var content = $(tr).find('.accounting-content .accounting-info').text()
	$(tr).find('.accounting-content .accounting-edit input').val(content)

	// 지출
	var outMoney = $(tr).find('.accounting-out-money').attr('data-out-money')
	$(tr).find('.accounting-out-money .accounting-edit input').val(outMoney)

	// 수입
	var inMoney = $(tr).find('.accounting-in-money').attr('data-in-money')
	$(tr).find('.accounting-in-money .accounting-edit input').val(inMoney)

	$(tr).find('.accounting-info').css('display', 'none')
	$(tr).find('.accounting-edit').css('display', 'block')
})

$(document).on('click', 'button.button-accounting-edit-save', function () {
	var tr = $(this).parent().parent().parent();
	$.post('fnb/accounting/edit', {
		id: $(tr).attr('data-accounting-id'),
		date: $(tr).find('.datetimepicker').attr('data-date'),
		content: $(tr).find('.accounting-content .accounting-edit input').val(),
		outMoney: $(tr).find('.accounting-out-money .accounting-edit input').val(),
		inMoney: $(tr).find('.accounting-in-money .accounting-edit input').val(),
	}, function (data) {
		if (data == 'success') {
			loadAccountingData();
		} else {
			showAlert(data)
		}
	})
})

$(document).on('click', 'button.button-accounting-edit-cancel', function () {
	var tr = $(this).parent().parent().parent();

	$(tr).find('.accounting-info').css('display', 'block')
	$(tr).find('.accounting-edit').css('display', 'none')
})

$(document).on('dblclick', 'button.button-accounting-delete', function () {
	$.post('fnb/accounting/delete', {
		id: $(this).attr('data-accounting-id')
	}, function (data) {
		if (data == 'success') {
			loadAccountingData();
		} else {
			showAlert(data)
		}
	})
})