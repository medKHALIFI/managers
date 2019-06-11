
function onSourceDownloadProgressChanged(sender, eventArgs) {
  sender.findName("uxStatus").Text = "Loading: " + Math.round((eventArgs.progress * 1000)) / 10 + "%";
  sender.findName("uxProgressBar").ScaleY = eventArgs.progress * 356;
}

function onLoaded(sender, eventArgs) {
  var splash = sender.findName("Splash");
  var width;
  var height;

  // Get the browser height and width.
  if (parseInt(navigator.appVersion) > 3) {
    if (navigator.appName == "Netscape") {
      width = window.innerWidth;
      height = window.innerHeight;
    }
  }

  if (navigator.appName.indexOf("Microsoft") != -1) {
    width = document.body.offsetWidth;
    height = document.body.offsetHeight;
  }

  splash["Canvas.Left"] = (width - splash.Width) / 2;
  splash["Canvas.Top"] = (height - splash.Height) / 2;
}