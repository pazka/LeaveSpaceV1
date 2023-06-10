import fs from "fs"
import * as satellite from "satellite.js" 

/*
 enrich elsets by adding simplier to use positional data using external library
*/


const file_path = "all_gp.json"
type Elset = {
    "CCSDS_OMM_VERS": string,
    "COMMENT": string,
    "CREATION_DATE": string,
    "ORIGINATOR": string,
    "OBJECT_NAME": string,
    "OBJECT_ID": string,
    "CENTER_NAME": string,
    "REF_FRAME": string,
    "TIME_SYSTEM": string,
    "MEAN_ELEMENT_THEORY": string,
    "EPOCH": string,
    "MEAN_MOTION": string,
    "ECCENTRICITY": string,
    "INCLINATION": string,
    "RA_OF_ASC_NODE": string,
    "ARG_OF_PERICENTER": string,
    "MEAN_ANOMALY": string,
    "EPHEMERIS_TYPE": string,
    "CLASSIFICATION_TYPE": string,
    "NORAD_CAT_ID": string,
    "ELEMENT_SET_NO": string,
    "REV_AT_EPOCH": string,
    "BSTAR": string,
    "MEAN_MOTION_DOT": string,
    "MEAN_MOTION_DDOT": string,
    "SEMIMAJOR_AXIS": string,
    "PERIOD": string,
    "APOAPSIS": string,
    "PERIAPSIS": string,
    "OBJECT_TYPE": string,
    "RCS_SIZE": string,
    "COUNTRY_CODE": string,
    "LAUNCH_DATE": string,
    "SITE": string,
    "DECAY_DATE": string,
    "FILE": string,
    "GP_ID": string,
    "TLE_LINE0": string,
    "TLE_LINE1": string,
    "TLE_LINE2": string
}

type FileData = {
    request_metadata: {
        "Total": number, "Limit": number, "LimitOffset": number, "ReturnedRows": number, "RequestTime": string, "DataSize": string
    }, data: Elset[]
}

type DataVisuElset = Elset & {
    DataVisu?: {
        projected2DX: number,
        projected2DY: number,
        projected2DRadius: number,
    }
}



//open file 
const file = fs.readFileSync(file_path, "utf-8")

//count json elems
const json: FileData = JSON.parse(file)

//print all keys except data
const all_data: Elset[] = json.data

const count = all_data.length

console.log(count)
console.log(all_data[0])


function getDataVisuElsetFromElset(elset : Elset) : DataVisuElset{
    const satrec = satellite.twoline2satrec(elset.TLE_LINE1, elset.TLE_LINE2);
    const positionAndVelocity = satellite.propagate(satrec, new Date(elset.EPOCH));
    const gmst = satellite.gstime(new Date(elset.EPOCH));
    const positionEci = positionAndVelocity.position;
    const velocityEci = positionAndVelocity.velocity;
    return {
        ...elset,
        DataVisu: {
            projected2DX: positionEci.x,
            projected2DY: positionEci.y,
            projected2DRadius: Math.sqrt(positionEci.x * positionEci.x + positionEci.y * positionEci.y)
        }
    }
}

let i = 0;
let start_time = new Date()
let unprocessable_elset = 0

const data_visu : DataVisuElset[] = all_data.map(elset => {
    i++
    // console.clear()
    // console.log(`processing ${i}/${count} (${Math.round(i/count*100)}%) estimated`, Math.round((new Date().getTime() - start_time.getTime()) / i * (count - i) / 1000), "s left")

    try {
        return getDataVisuElsetFromElset(elset)
    } catch (e) {
        unprocessable_elset++
        return {...elset,COMMENT: "unprocessable"}
    }
})



console.log(data_visu[0])
console.log("gone through ",i," elems")
console.log("unprocessed : ",unprocessable_elset)

const data_visu_json = JSON.stringify(data_visu)
fs.writeFileSync("all_gp_data_visu.json", data_visu_json, "utf-8")