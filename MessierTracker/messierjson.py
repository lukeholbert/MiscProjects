# imports
from math import *
import json
import os
import sidereal
import shutil
from datetime import datetime

# Constants (Currently set to Pullman, WA. Change for outside use)
sd = sidereal
LAT = '46.729777dN'
LON = '117.181738dW'
lat = sd.parseLat(LAT)
lon = sd.parseLon(LON)

def main():

    objs = PopulateMessList(open("messier.json"))
    uplist = ["Info.txt"]
    for mess in objs:
        if CheckInSky(mess):
            uplist.append(mess.mess + ".jpg")
            
    # First, go through files in directory
    # Delete every file that isn't in uplist
    # Else, remove that filename from uplist
    for file in os.listdir("CurrentSky"):
        if not file in uplist:
            os.remove("CurrentSky/" + file)
            print(file + " removed")
        else:
            uplist.remove(file)            

    # uplist now has every file that needs to be moved
    for obj in uplist:
        file = "MessierImages/" + obj
        shutil.copy(file, "CurrentSky")
        
        
def CheckInSky(messobj):
    ra = sd.hoursToRadians(RAToDegrees(messobj.ra))
    dec = (DecToDegrees(messobj.dec) * pi) / 180.0
    radec = sd.RADec(ra, dec)
    hourang = radec.hourAngle(datetime.utcnow(), lon)
    altaz = radec.altAz(hourang, lat)
    if (float)(altaz.alt) > 0:
        return True
    return False
        
def PopulateMessList(file):
    js = json.loads(file.read())
    objlist = []
    for obj in js["MessierObjects"]:
        objlist.append(AsMessobj(obj))
    return objlist
    
class MessierObj(object):
    def __init__(self, mess, ngc, const, ra, dec, desc):
        self.mess = mess
        self.ngc = ngc
        self.const = const
        self.ra = ra
        self.dec = dec
        self.desc = desc

def AsMessobj(dic):
    return MessierObj(dic['Messier'], dic['NGC'], dic['Const'], dic['RA'], dic['Dec'], dic['Desc'])

def RAToDegrees(ra):
    return (float)(ra[0:2]) + ((float)(ra[2:4])/60) + ((float)(ra[4:6])/3600)

def DecToDegrees(dec):
    return (float)(dec[0:3]) + ((float)(dec[3:5])/60) + ((float)(dec[5:7])/3600)

if __name__ == "__main__":
    main()
