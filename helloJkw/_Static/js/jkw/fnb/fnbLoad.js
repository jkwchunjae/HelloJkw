var app = angular.module('app-fnb', []);
var getScope = function () {
	return angular.element(document.querySelector('[ng-controller="fnb-controller"]')).scope();
};

app.controller('fnb-controller', function ($scope) {
	$scope.memberListRegular = new Array();
	$scope.memberListAssociate = new Array();
	$scope.meetingList = new Array();
	$scope.accountingDataList = new Array();
	$scope.totalOutMoney = '0';
	$scope.totalInMoney = '0';

	loadMemberRegular(); //  정회원
	loadMemberAssociate(); //  준회원
	loadMeeting(); //  모임
	loadAccountingData(); //  회계
});

function loadMemberRegular () {
	var scope = getScope();
	$.post('fnb/member/request', {
		memberType: 'Regular'
	}, function (data) {
		scope.memberListRegular = JSON.parse(data);
		scope.$apply();
	});
};

function loadMemberAssociate () {
	var scope = getScope();
	$.post('fnb/member/request', {
		memberType: 'Associate'
	}, function (data) {
		scope.memberListAssociate = JSON.parse(data);
		scope.$apply();
	});
};

function loadMeeting() {
	var scope = getScope();
	$.post('fnb/meeting/request', {
		// params
	}, function (data) {
		scope.meetingList = JSON.parse(data);
		scope.$apply();

		$('.meeting-others .meeting-info').each(function () {
			var newOthers = $(this).text().replace(/(?:\r\n|\r|\n)/g, '<br/>')
			$(this).html(newOthers)
		})
	});
};

function loadAccountingData() {
	var scope = getScope();
	$.post('fnb/accounting/request', {
		// params
	}, function (data) {
		var obj = JSON.parse(data);
		scope.accountingDataList = obj.accountingDataList;
		scope.totalOutMoney = obj.totalOutMoney;
		scope.totalInMoney = obj.totalInMoney;
		scope.$apply();
	});
};

function getMeeting(meetingNo){
	var scope = getScope()

	for (var index in scope.meetingList) {
		var meeting = scope.meetingList[index]
		if (meeting.No == meetingNo) {
			return meeting
		}
	}
}