// Segment analytics
var hostname = window.location.hostname;
if (hostname !== "localhost" && hostname !== "127.0.0.1") {
  !function(){
    var analytics=window.analytics=window.analytics||[];
    if(!analytics.initialize)if(analytics.invoked)window.console&&console.error&&console.error("Segment snippet included twice.");
    else{
      analytics.invoked=!0;
      analytics.methods=["trackSubmit","trackClick","trackLink","trackForm","pageview","identify","reset","group","track","ready","alias","debug","page","once","off","on"];
      analytics.factory=function(t){
        return function(){
          var e=Array.prototype.slice.call(arguments);
          e.unshift(t);
          analytics.push(e);
          return analytics
        }
      };
      for(var t=0;t<analytics.methods.length;t++){
        var e=analytics.methods[t];
        analytics[e]=analytics.factory(e)
      }
      analytics.load=function(t){
        var e=document.createElement("script");
        e.type="text/javascript";
        e.async=!0;
        e.src=("https:"===document.location.protocol?"https://":"http://")+"cdn.segment.com/analytics.js/v1/"+t+"/analytics.min.js";
        var n=document.getElementsByTagName("script")[0];
        n.parentNode.insertBefore(e,n)
      };
      analytics.SNIPPET_VERSION="4.0.0";
      analytics.load("WJbiYsaHxarqlWABHccBkGaB0tTNp1Rb");
      analytics.page();
    }
  }();
}

// Handle code tabs
this.Tabs = (function() {
  Tabs.prototype.navTabs = null;
  Tabs.prototype.panels = null;
  function Tabs(elem) {
    this.navTabs = elem.getElementsByClassName('nav-item');
    this.panels = elem.getElementsByClassName('tab-pane');
    this.bind();
  };
  Tabs.prototype.show = function(index) {
    var activePanel, activeTab;
    for (var i = 0, l = this.navTabs.length; i < l; i++) {
      this.navTabs[i].classList.remove('active');
    }
    activeTab = this.navTabs[index];
    activeTab.classList.add('active');
    for (var i = 0, l = this.panels.length; i < l; i++) {
      this.panels[i].classList.remove('active');
    }
    activePanel = this.panels[index];
    return activePanel.classList.add('active');
  };
  Tabs.prototype.bind = function() {
    var _this = this;
    for (var i = 0, l = this.navTabs.length; i < l; i++) {
      (function(elem, index) {
        elem.addEventListener('click', function(evt) {
          evt.preventDefault();
          return _this.show(index);
        });
      })(this.navTabs[i], i);
    }
  };
  return Tabs;
})();

document.addEventListener('DOMContentLoaded', function() {
  var nodes = document.getElementsByClassName('code-nav-container');
  for (var i = 0, l = nodes.length; i < l; i++) {
    new Tabs(nodes[i]);
  }
});
