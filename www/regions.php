<? 
//-----------------------------------------------------------------------------
// $Workfile: imagetest.php $ $Revision: 0.0 $ $Author: addy $ 
// $Date: 2006/04/16 23:46:04 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL & ~E_NOTICE);
require_once('./global.php');


class Region
{
	var $id;
	var $name;
	var $coordsArray = array();
	var $population;	
	
	function pushCoords($coords)
	{
		array_push($this->coordsArray,$coords);
	}
	
   function setProp($PropName, $PropValue) {
       $this->$PropName = $PropValue;
   }	
}

class RegionLoader
{
	
	function loadRegion($key,$id)
	{
		
	}
	
}

?>