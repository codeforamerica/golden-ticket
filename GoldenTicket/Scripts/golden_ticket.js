$( document ).bind( "enhance", function(){
  $( "body" ).addClass( "enhanced" );
  //$( "input[type=radio]" ).customInput();
  //$( "input[type=checkbox]" ).customInput();
  //$( "body" ).addClass( "custom-input" );
});

$(document).ready(function(){
  $('body').removeClass('no-js');
  $( document ).trigger( "enhance" );
});