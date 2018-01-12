var timer = null;
var curStyle = "day";
var gx = 0;
var gy = 0;
var input_date;


function fun1(evnt) 
{   
	gx = evnt.pageX;
 	gy = evnt.pageY;
 	return true;   
}
if(navigator.appName.indexOf("Netscape") != -1) {
document.onmousemove = fun1;
}

var d = new Date();
var dc;
var monthname=new Array("Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec");
var data = new Array(6);
data[0] = new Array(0,0,0,0,0,0,0);
data[1] = new Array(0,0,0,0,0,0,0);
data[2] = new Array(0,0,0,0,0,0,0);
data[3] = new Array(0,0,0,0,0,0,0);
data[4] = new Array(0,0,0,0,0,0,0);
data[5] = new Array(0,0,0,0,0,0,0);


isDOM = (document.getElementById) ? true : false;
isNS4 = (document.layers) ? true : false;
isIE = (document.all) ? true : false;
isIE4 = isIE && !isDOM;
isMac = (navigator.appVersion.indexOf("Mac") != -1);
isIE4M = isIE4 && isMac;


isOpera = (navigator.userAgent.indexOf("Opera")!=-1);
isKonqueror = (navigator.userAgent.indexOf("Konqueror")!=-1);
isMenu = !isOpera && !isKonqueror && !isIE4M && (isDOM || isNS4 || isIE4);
isNS6 = (navigator.vendor == ("Netscape6") || navigator.product == ("Gecko"));
isBrowserString = isNS4 ? "ns4" : isDOM ? "dom" : "ie4";

function msOver(td) {
   curStyle = td.className;
   td.className = "day_c";
}

function msOut(td) {   td.className = curStyle; }


function getRealLeft(el) {
    xPos = el.offsetLeft;
    tempEl = el.offsetParent;
    while (tempEl != null) {
        xPos += tempEl.offsetLeft;
        tempEl = tempEl.offsetParent;
    }
    return xPos;
}

function getRealTop(el) {
    yPos = el.offsetTop;
    tempEl = el.offsetParent;
    while (tempEl != null) {
        yPos += tempEl.offsetTop;
        tempEl = tempEl.offsetParent;
    }
    return yPos;
}


function PlaceAtElement(elementid,pos){
	var TheAnchor = isDOM ? document.getElementById(elementid) : isIE4 ? document.all(elementid) : document.anchors[elementid];
	if(isNS6){
		TheAnchor.style.position = "relative";
		MarginWidth = parseInt(document.body.getAttribute("marginwidth"));
		MarginWidth = (isNaN(MarginWidth)) ? 8 : MarginWidth;
	}

	switch (pos){
		case "left":
			retVal = isIE ? getRealLeft(TheAnchor) : isNS4 ? TheAnchor.x : TheAnchor.offsetLeft + MarginWidth - 8 ;
			break;
		case "top":
			retVal = isIE ? getRealTop(TheAnchor)+18 : isNS4 ? TheAnchor.y+10 : TheAnchor.offsetTop + TheAnchor.offsetHeight;
			break;
	}
	return retVal;
}

function chg(k) {
 d.setMonth(d.getMonth() + k);
 document.getElementById('month_year').innerHTML = monthname[d.getMonth()] + ' ' + d.getFullYear();
 var dd = new Date(d);
for (i=0;i<6;i++)
 	for (j=0;j<7;j++) data[i][j] = 0;
 dd.setDate(1);
 i = 0;
 do {
   data[i][dd.getDay()] = dd.getDate();
   if (dd.getDay() == 6) i++;
   dd.setDate(dd.getDate() + 1);
 }  while (dd.getDate() != 1);
 for (i=0;i<6;i++)
 	for (j=0;j<7;j++) 
	    if (data[i][j] == 0) {
	document.getElementById("c" +i+""+j).innerHTML= "&nbsp;";
	document.getElementById("c" +i+""+j).className = "day_out";
	}
	else
	{
	document.getElementById("c" +i+""+j).innerHTML= data[i][j];
	document.getElementById("c" +i+""+j).className = "day";
	
	if (dc != undefined)
	 if (dc.getDate() ==  data[i][j])
		document.getElementById("c" +i+""+j).className = "day_c";
	}
 }

function setDate(i,j) {
if (data[i][j]!=0) 
{
	m = d.getMonth()+1;
	
	stub = "";
	if (m < 10)
		stub = "0";

	stub2 = "";
	if (data[i][j] < 10)
		stub2 = "0";

	input_date.value =  d.getFullYear() + "/" + stub + m + "/" + stub2 + data[i][j];}
	hd();
}

function getDate(cel) {
input_date = cel;
if (cel.value.length !=0) {
	d = new Date(cel.value); 
	if (d == 'NaN') d = new Date();
	dc = new Date(d); }
chg(0);
document.getElementById('avkCalendar').style.left=PlaceAtElement(cel.name,'left');
document.getElementById('avkCalendar').style.top=PlaceAtElement(cel.name,'top');
document.getElementById('avkCalendar').style.visibility = "visible";

}

function hd() 
{
	document.getElementById('avkCalendar').style.visibility='hidden';
}

function setHandler() 
{
	return "Finish decoding.";
}
