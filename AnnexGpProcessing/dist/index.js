"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || function (mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (k !== "default" && Object.prototype.hasOwnProperty.call(mod, k)) __createBinding(result, mod, k);
    __setModuleDefault(result, mod);
    return result;
};
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const fs_1 = __importDefault(require("fs"));
const satellite = __importStar(require("satellite.js"));
/*
 enrich elsets by adding simplier to use positional data using external library
*/
const file_path = "all_gp.json";
//open file 
const file = fs_1.default.readFileSync(file_path, "utf-8");
//count json elems
const json = JSON.parse(file);
//print all keys except data
const all_data = json.data;
const count = all_data.length;
console.log(count);
console.log(all_data[0]);
function getDataVisuElsetFromElset(elset) {
    const satrec = satellite.twoline2satrec(elset.TLE_LINE1, elset.TLE_LINE2);
    const positionAndVelocity = satellite.propagate(satrec, new Date(elset.EPOCH));
    const gmst = satellite.gstime(new Date(elset.EPOCH));
    const positionEci = positionAndVelocity.position;
    const velocityEci = positionAndVelocity.velocity;
    return Object.assign(Object.assign({}, elset), { DataVisu: {
            projected2DX: positionEci.x,
            projected2DY: positionEci.y,
            projected2DRadius: Math.sqrt(positionEci.x * positionEci.x + positionEci.y * positionEci.y)
        } });
}
let i = 0;
let start_time = new Date();
let unprocessable_elset = 0;
const data_visu = all_data.map(elset => {
    i++;
    // console.clear()
    // console.log(`processing ${i}/${count} (${Math.round(i/count*100)}%) estimated`, Math.round((new Date().getTime() - start_time.getTime()) / i * (count - i) / 1000), "s left")
    try {
        return getDataVisuElsetFromElset(elset);
    }
    catch (e) {
        unprocessable_elset++;
        return Object.assign(Object.assign({}, elset), { COMMENT: "unprocessable" });
    }
});
console.log(data_visu[0]);
console.log("gone through ", i, " elems");
console.log("unprocessed : ", unprocessable_elset);
const data_visu_json = JSON.stringify(data_visu);
fs_1.default.writeFileSync("all_gp_data_visu.json", data_visu_json, "utf-8");
