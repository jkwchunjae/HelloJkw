
$(document).on({
	mouseenter: function () {
		$(this).children('.button-list').css('visibility', 'visible');
	},
	mouseleave: function () {
		$(this).children('.button-list').css('visibility', 'hidden');
	}
}, '.member');

$('.datetimepicker').datetimepicker({
	useStrict: true,
	format: 'YYYY-MM-DD'
}).on('dp.change', function (e) {
	$(this).attr('data-date', getDate8(e.date));
});

$('.datetimepicker input').keydown(function () {
	return false;
});

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

$(document).on('click', 'button.btn-member-leave', function () {
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
	// 이게 메인
	var fnAddMeeting = function () {
		var attendantsArray = new Array();
		$('button.add-meeting-member-button').each(function () {
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
	};

	fnAddMeeting();
});

$(document).on('click', 'button.button-delete-meeting', function () {
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

$(document).on('click', 'button.button-accounting-delete', function () {
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