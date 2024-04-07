import fs from "fs"

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


//open file 
const file = fs.readFileSync(file_path, "utf-8")

//count json elems
const json: FileData = JSON.parse(file)

//print all keys except data
const all_data: Elset[] = json.data

const count = all_data.length

console.log(count)
console.log(all_data[0])

//Count huw much of each OBJECT_TYPE there is
const object_types: string[] = all_data.map((el: Elset) => el.OBJECT_TYPE)
const object_types_set: string[] = [...new Set(object_types)]
//add the object_type but decayed to the list
const object_types_and_decayed: string[] = []
all_data.forEach((el: Elset) => {
    if (is_decayed(el)) {
        object_types_and_decayed.push(el.OBJECT_TYPE + "_decayed")
    } else {
        object_types_and_decayed.push(el.OBJECT_TYPE)
    }
})
    

const object_types_count: { [key: string]: number } = {}

object_types_and_decayed.forEach((el: string) => {
    if (object_types_count[el]) {
        object_types_count[el] += 1
    } else {
        object_types_count[el] = 1
    }
}
)

function decay_date_to_date(decay_date: string): Date {
    //fromat is YYYY-MM-DD
    const parts = decay_date.split('-');
    const year = parseInt(parts[0], 10);
    const month = parseInt(parts[1], 10) - 1; // Months in JavaScript are zero-based
    const day = parseInt(parts[2], 10);

    const dateObject = new Date(year, month, day);
    return dateObject
}

function is_decayed(el: Elset): boolean {
    //fromat is YYYY-MM-DD
    return (el.DECAY_DATE != null && el.DECAY_DATE !== "" && el.DECAY_DATE !== "0000-00-00" && decay_date_to_date(el.DECAY_DATE) < new Date())
}

console.log(object_types_count)

//print in csv with column : active and decayed and total, and each line for the object type
const lines: any[][] = []
object_types_set.forEach((key: string) => {
    lines.push([key, object_types_count[key] ,object_types_count[key + "_decayed"] ,(object_types_count[key] + object_types_count[key + "_decayed"])])
})

const csv = "TYPE,ACTIVE,DECAYED,TOTAL\n"+lines.map((el: any[]) => el.join(",")).join("\n")

fs.writeFileSync("object_types_count.csv", csv)