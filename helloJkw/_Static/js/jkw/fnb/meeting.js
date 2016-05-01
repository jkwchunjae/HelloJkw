var appMeeting = angular.module('app-meeting', []);
var getScopeMeeting = function () {
	return angular.element(document.querySelector('[ng-controller="meeting-controller"]')).scope();
};

appMeeting.controller('meeting-controller', function ($scope) {
	$scope.meetinglist = new Array();

	$.post('fnb/meeting/request', {
		// params
	}, function (data) {
		alert(data);
		$scope.meetinglist = JSON.parse(data);
		$scope.$apply();
	});
});