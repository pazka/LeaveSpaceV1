import fs from 'fs';

const allTLEs = fs.readFileSync('data/allSatelites_17022024.txt', 'utf-8').split('\n');

const satelites = allTLEs.reduce((acc: any[], line, index) => {
    if (index % 2 === 0) {
        acc.push([allTLEs[index], allTLEs[index + 1]]);
    }
    return acc;
}, []);

const whichOrbit :any={
    "LEO" : {amount : 0,satelites : []},
    "MEO" : {amount : 0,satelites : []},
    "GEO" : {amount : 0,satelites : []},
    "HEO" : {amount : 0,satelites : []},
}

const giveSateliteOrbit = (satelite : any) => {
    if (satelite[1] == null) {
      console.log(satelite)
      return
    }
    const lineData = satelite[1].split(/\s+/);
    const meanMotion = lineData[6];
    if(meanMotion >= 12){
        whichOrbit.LEO.amount += 1;
        //whichOrbit.LEO.satelites.push(satelite);
    } else if(meanMotion > 2 && meanMotion < 12){
        whichOrbit.MEO.amount += 1;
       // whichOrbit.MEO.satelites.push(satelite);
    } else if(meanMotion < 2 && meanMotion > 0.8){
        whichOrbit.GEO.amount += 1;
       // whichOrbit.GEO.satelites.push(satelite);
    } else {
        whichOrbit.HEO.amount += 1;
       // whichOrbit.HEO.satelites.push(satelite);
    }
}

satelites.forEach((satelite) => {
    giveSateliteOrbit(satelite);
});

/* Our specific TLE description 
    * 0: Line 1 [Satelite catalog number/Classification] [Launch year/Launch number/Piece of the launch] [Epoch year/Epoch day] [First time derivative of the mean motion] [Second time derivative of the mean motion] [BSTAR drag term] [Ephemeris type] [Element set number]
    * 1: Line 2 [Satelite catalog number] [Inclination] [Right ascension of the ascending node] [Eccentricity] [Argument of perigee] [Mean anomaly] [Mean motion] [Revolution number at epoch]
    */

const satelitesObjs : any[]= satelites.map((satelite) => {
    const lineData = satelite[0].split(/\s+/);

    return {
        elementSetNumber : lineData[7],
    };
});

const elementSetNumberCount = satelitesObjs.reduce((acc: any, satelite) => {
    if (acc[satelite.elementSetNumber]) {
        acc[satelite.elementSetNumber] += 1;
    } else {
        acc[satelite.elementSetNumber] = 1;
    }
    return acc;
}, {});

//dump json 
fs.writeFileSync('data/satelites.json', JSON.stringify(satelitesObjs, null, 2));

//dump element set number count
fs.writeFileSync('data/elementSetNumberCount.json', JSON.stringify(elementSetNumberCount, null, 2));

//dump orbit classification

fs.writeFileSync('data/whichOrbit.json', JSON.stringify(whichOrbit, null, 2));