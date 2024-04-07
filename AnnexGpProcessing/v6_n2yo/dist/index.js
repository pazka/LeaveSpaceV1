"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const fs_1 = __importDefault(require("fs"));
const allTLEs = fs_1.default.readFileSync('data/allSatelites_17022024.txt', 'utf-8').split('\n');
const satelites = allTLEs.reduce((acc, line, index) => {
    if (index % 2 === 0) {
        acc.push([allTLEs[index], allTLEs[index + 1]]);
    }
    return acc;
}, []);
/* Our specific TLE description
    * 0: Line 1 [Satelite catalog number][Classification] [Launch year][Launch number][Piece of the launch] [Epoch year][Epoch day] [First time derivative of the mean motion] [Second time derivative of the mean motion] [BSTAR drag term] [Ephemeris type] [Element set number]
    * 1: Line 2 [Satelite catalog number] [Inclination] [Right ascension of the ascending node] [Eccentricity] [Argument of perigee] [Mean anomaly] [Mean motion] [Revolution number at epoch]
    */
const satelitesObjs = satelites.map((satelite) => {
    const obj = {
        catalogNumber: satelite[0].slice(2, 7),
        classification: satelite[0].slice(7, 8),
        launchYear: satelite[0].slice(9, 11),
        launchNumber: satelite[0].slice(11, 14),
        pieceOfLaunch: satelite[0].slice(14, 17),
        epochYear: satelite[0].slice(18, 20),
        epochDay: satelite[0].slice(20, 32),
        firstTimeDerivative: satelite[0].slice(33, 43),
        secondTimeDerivative: satelite[0].slice(44, 52),
        bstarDragTerm: satelite[0].slice(53, 61),
        ephemerisType: satelite[0].slice(62, 63),
        elementSetNumber: satelite[0].slice(64, 68),
        inclination: satelite[1].slice(8, 16),
        rightAscension: satelite[1].slice(17, 25),
        eccentricity: satelite[1].slice(26, 33),
        argumentOfPerigee: satelite[1].slice(34, 42),
        meanAnomaly: satelite[1].slice(43, 51),
        meanMotion: satelite[1].slice(52, 63),
        revolutionNumberAtEpoch: satelite[1].slice(63, 68),
    };
    return obj;
});
const elementSetNumberCount = satelitesObjs.reduce((acc, satelite) => {
    if (acc[satelite.elementSetNumber]) {
        acc[satelite.elementSetNumber] += 1;
    }
    else {
        acc[satelite.elementSetNumber] = 1;
    }
    return acc;
}, {});
//dump json 
fs_1.default.writeFileSync('data/satelites.json', JSON.stringify(satelitesObjs, null, 2));
//dump element set number count
fs_1.default.writeFileSync('data/elementSetNumberCount.json', JSON.stringify(elementSetNumberCount, null, 2));
