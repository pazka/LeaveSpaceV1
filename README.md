# LeaveSpace Visualisation

## Description

This project will provide a visualisation of the space-track data, and eventually the JSC Vimpel.

It will then be projected on a sculpture made by the artist Alessia Sanna.

## Visualisation

The data is recorded then converted on a 2D plane given some specific rules.

- The satelittes on the LEO orbit will take about 80% of the available space
- The aspect of the dot will chnage given some specific rules(stelite name, constellation size, etc...)
- All decayed object will not be displayed

## Processing

### First draft

the data is pre-processed in json by the `satellite.js` [link](https://github.com/shashwatak/satellite-js) library.

we simply add the X,Y coordinates as well as the circle radius to the json file.

Then we move the object in a circle around the earth, same speed for every objects but different raiduses

### End Goal

The position will be projected on a 2D ellipse coming from the Orbital Data from the TLE.
Then we uses those data to place the object and move in around ( the meaning of General Propagation) in the visualisation

## TODO

### V0.1

- ✅Collect and Clean Data
- ✅create first visualisation
- ✅Change color reflecting usage
- ✅Loop visual ( slow backward, normal forward)
- ✅Add sounds from Stephane Clor
- ✅Loop visual ( slow backward, normal forward)
- ✅Add sounds from Stephane Clor

### V1

- ✅ musk apparait rapide ( 3* plus rapide stalittes)
- ✅ apprition the fait proportionnelement à T en spiral evers l'intérieur
- ✅ apparition musk déclhenche bleuisement
- ✅ Musk apparait avec son orbite
- ✅ apparition musk accélère le visuel
- ✅ Musk extrapolated
- ✅ 1 musk aparait -> increase chance of debris apparition 
- ✅ debris apparait -> increase chance of debris apparition

### V1.5

- ✅ Make first sequence increase speed gradually 
- ✅ make first sequence dots change size gradually
- [] Link star size to distance from earth of the object