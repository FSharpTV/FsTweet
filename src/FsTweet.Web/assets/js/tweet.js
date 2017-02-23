$(function(){
  window.renderTweets = function(tweets, $el) {
    var timeAgo = function () {
      return function(val, render) {
        return moment(render(val)).fromNow()
      };
    }

    var tweetReadTmpl = `
      {{#tweets}}
        <div>        
          <div class="tweet_read_view bg-info">
            <span class="text-muted">@{{username.value}} - {{#timeAgo}}{{time}}{{/timeAgo}}</span>
            <p>{{tweet.value}}</p>
          </div>        
        </div>

      {{/tweets}}
    `

    if (tweets.length === 0) {
        $el.html('<p class="bg-warning no_tweets">No tweets found <p></div>')
        return
      }
      var htmlOutput = Mustache.render(tweetReadTmpl, {
        "tweets" : tweets,
        "timeAgo" : timeAgo
      });
      $el.html(htmlOutput)
  }

  var $tweetForm = $(".form-tweet")
  $tweetForm.submit(function(e){
    $.ajax({
      type: $tweetForm.attr('method'),
      url: $tweetForm.attr('action'),
      data: $tweetForm.serialize(),
      success: function (data) {          
          $("#tweet").val('');
          $(document).trigger("tweet:posted")
      },
      error: function(data) {
        alert(data.responseText)
      }
    });
    e.preventDefault()
  });
})