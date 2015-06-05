
$(document).ready(function () {
	$('.problem div:nth-child(1)').addClass('question');
	$('.problem div:nth-child(2)').addClass('solution');

	// 정답 보기 버튼
	var showButton = document.createElement('button');
	$(showButton)
		.text('정답보기')
		.css('display', 'block')
		.addClass('btn').addClass('btn-danger')
		.on('click', function () {
			var answer = $(this).parent().siblings('.solution');
			$(answer).css('display', 'block');
			$(this).css('display', 'none');
		});
	$('div.problem .question').append(showButton);

	$('.exam div:nth-child(1)').addClass('question');
	$('.exam div:nth-child(2)').addClass('solution');

	// 정답입력 div
	var inputAnswer = document.createElement('input');
	$(inputAnswer).attr('type', 'text')
		.css('width', 'initial')
		.addClass('form-control')
		.addClass('input-answer');
	var buttonCheckAnswer = document.createElement('button');
	$(buttonCheckAnswer).attr('type', 'button')
		.addClass('btn-check-answer')
		.addClass('btn')
		.addClass('btn-danger')
		.text('정답확인');
	var buttonShowSolution = document.createElement('button');
	$(buttonShowSolution).attr('type', 'button')
		.addClass('btn-show-solution')
		.addClass('btn')
		.addClass('btn-success')
		.text('해설보기');
	var spanInputGroup = document.createElement('span');
	$(spanInputGroup).addClass('input-group-btn')
		.css('width', 'initial')
		.append(buttonCheckAnswer)
		.append(buttonShowSolution);
	var divInputGroup = document.createElement('div');
	$(divInputGroup).addClass('input-group')
		.css('display', 'inline-flex')
		.append(inputAnswer)
		.append(spanInputGroup);
	var divAnswer = document.createElement('div');
	$(divAnswer).addClass('answer')
		.append(divInputGroup)
		.insertAfter('.exam .question');

	var btnMessage = document.createElement('button');
	$(btnMessage).attr('type', 'button')
		.addClass('btn');
	var divMessage = document.createElement('div');
	$(divMessage)
		.addClass('message')
		.css('display', 'none')
		.append(btnMessage)
		.insertAfter('.exam .answer');




	// Qn. An. Sn. 번호 표시
	$('.question').each(function (index) {
		var markQ = document.createElement('span');
		$(markQ).addClass('markProblem').addClass('markQ')
			.html('Q<sub>' + (index + 1) + '</sub>.');
		$(this).prepend(markQ);

		var markA = document.createElement('span');
		$(markA).addClass('markProblem').addClass('markA')
			.html('A<sub>' + (index + 1) + '</sub>.');
		var answer = $(this).siblings('.answer');
		$(answer).prepend(markA);

		var markS = document.createElement('span');
		var mark = ($(this).parent().hasClass('problem')) ? 'A' : 'S';
		$(markS).addClass('markProblem').addClass('markS')
			.on('click', function () { // An. 을 누르면 정답이 숨겨지도록!
				$(this).parent().css('display', 'none');
				$(this).parent().siblings('.question').children('button').css('display', 'block');
			})
			.html(mark + '<sub>' + (index + 1) + '</sub>.');
		var solution = $(this).siblings('.solution');
		$(solution).css('display', 'none').prepend(markS);
	});

	$('.exam').each(function () {
		var input = $(this).find('input');
		var question = $(this).find('.question');
		var solution = $(this).find('.solution');
		var message = $(this).find('.message');
		$(input).attr('placeholder', $(question).attr('data-format'));

		var showSolution = function () {
			$(solution).css('display', 'block');
		};

		var btnCheckAnswer = $(this).find('button.btn-check-answer');
		$(btnCheckAnswer).on('click', function () {
			var checkAnswer = function (userAnswer) {
				var correctAnswer = $(question).attr('data-answer');
				return userAnswer == correctAnswer;
			};
			var pass = function () {
				var button = $(message).find('button');
				$(button).removeClass('btn-warning').addClass('btn-success').text('정답입니다!');
				$(message).fadeIn(function () {
					window.setTimeout(function () {
						$(message).fadeOut(showSolution);
					}, 1000);
				});
			};
			var fail = function () {
				var button = $(message).find('button');
				$(button).removeClass('btn-success').addClass('btn-warning').text('틀렸습니다!');
				$(message).fadeIn(function () {
					window.setTimeout(function () {
						$(message).fadeOut();
					}, 1000);
				});
			};
			if (checkAnswer($.trim($(input).val()))) {
				pass();
			} else {
				fail();
			}
		});

		var btnShowSolution = $(this).find('button.btn-show-solution');
		$(btnShowSolution).on('click', showSolution);
	});
});