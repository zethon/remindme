function outputList(ar, name, size) {
 var strIDs = "<SELECT SIZE=\"" + size + "\" NAME=\"ro_lst" + name + "\">"
 var sel = " SELECTED"
 for (var i=0;i<ar.length;i++) {
  strIDs += "<OPTION " + sel + " VALUE=\"" + ar[i][0] + "\">" + ar[i][1]
  sel = ""
 }
 strIDs+="</SELECT>"
 strIDs+="<INPUT NAME=\"" + name + "\" TYPE=hidden>"
 return strIDs
}

function outputButton(bDir,name,val) {
 return "<INPUT TYPE=button VALUE=\"" + val + "\" ONCLICK=\"move(this.form," + bDir + ",'" + name + "')\">"
}

function move(f,bDir,sName) {
 var el = f.elements["ro_lst" + sName]
 var idx = el.selectedIndex
 if (idx==-1) 
  alert("You must first select the item to reorder.")
 else {
  var nxidx = idx+( bDir? -1 : 1)
  if (nxidx<0) nxidx=el.length-1
  if (nxidx>=el.length) nxidx=0
  var oldVal = el[idx].value
  var oldText = el[idx].text
  el[idx].value = el[nxidx].value
  el[idx].text = el[nxidx].text
  el[nxidx].value = oldVal
  el[nxidx].text = oldText
  el.selectedIndex = nxidx
 }
}

function processForm(f) {
 for (var i=0;i<f.length;i++) {	
  var el = f[i]
  if (el.name.substring(0,6)=="ro_lst") {
   var strIDs = ""
   for (var j=0;j<f[i].options.length;j++)
     strIDs += f[i].options[j].value + ", "
   f.elements[f.elements[i].name.substring(6)].value = strIDs.substring(0,strIDs.length-2)
  }
 }
}