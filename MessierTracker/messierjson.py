# imports
import json
import sidereal

# Constants (Currently set to Pullman, WA. Change for outside use)
LAT = 46.729777
LONG = -117.181738

def main():
    
    objs = PopulateMessList(open("messier.json"))
    for mess in objs:
        if CheckInSky(mess):
            print(mess.mess + " " +  mess.desc)
        
def CheckInSky(messobj):
    # Check
    return True
        
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

if __name__ == "__main__": main()

