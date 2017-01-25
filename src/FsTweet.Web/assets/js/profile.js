$(function(){

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

  var timeAgo = function () {
    return function(val, render) {
      return moment(render(val)).fromNow()
    };
  }

  var username = window.location.pathname.substring(1)

  function loadTweets(username) {
    $.getJSON("/tweets/" + username, function(){

    }).done(function(data){
      if (data.length === 0) {
        $("#tweets").html('<p class="bg-warning no_tweets">No tweets found <p></div>')
        return
      }
      var htmlOutput = Mustache.render(tweetReadTmpl, {
        "tweets" : data,
        "timeAgo" : timeAgo
      });
      $("#tweets").html(htmlOutput)
    }).fail(function(data){
      alert(data)
    });
  }

  loadTweets(username)

  $(document).on('tweet:posted', function(){
    loadTweets(username)
  })

});