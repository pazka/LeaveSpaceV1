import fs from "fs"

/*
Group all elset by name and same launch ate to create constellation

Possibility to modify the code and obtain groups by name alone which crate larges ocnstelations
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


//open file 
const file = fs.readFileSync(file_path, "utf-8")

//count json elems
const json: FileData = JSON.parse(file)

//print all keys except data
const all_data: Elset[] = json.data

const count = all_data.length

console.log(count)
console.log(all_data[0])

function areNamesLooselyTheSame(name1: string, name2: string, max_errors: number, range: number) {
    let errors = 0

    let names1 = name1.split(/[\s-_#()/]/g).map(n => n.trim().replace(/[\d]/g, '')).filter(x => x)
    let names2 = name2.split(/[\s-_#()/]/g).map(n => n.trim().replace(/[\d]/g, '')).filter(x => x)

    if (names1.find(n => names2.includes(n)) || names2.find(n => names1.includes(n))) {
        return true
    }
    return false

    for (let i = 0; i < name1.length; i++) {
        let start_index = i - range < 0 ? 0 : i - range
        let end_index = i + range > name2.length ? name2.length : i + range

        let sub_name = name2.substring(start_index, end_index)
        if (sub_name.includes(name1[i])) {
            continue
        } else {
            errors++
        }
        if (errors > max_errors) {
            return false
        }
    }
    return errors <= max_errors
}

function groupElsetByLooseName(data: Elset[],dontUseCache : boolean = false): { [name: string]: Elset[] } {
    let groups: { [name: string]: Elset[] } = {}
    const filename = "grouped_elsets.json"

    //if result file exist, just read it
    if (!dontUseCache && fs.existsSync(filename)) {
        return JSON.parse(fs.readFileSync(filename, "utf-8"))
    }

    let alreadyprocessedNames: string[] = []

    for (let i = 0; i < data.length; i++) {
        console.log(i, "/", data.length)
        let fullname = data[i].OBJECT_NAME
        let name = fullname.replace(/[\d\s]/g, "")
        let date = data[i].LAUNCH_DATE

        let found = false
        for (let key in groups) {
            let key_name = key.split('##')[0]
            let key_date = key.split('##')[1]
            if (key_date == date && areNamesLooselyTheSame(name, key_name, 4, 2)) {
                groups[key].push(data[i])
                // console.log("found",groups[key].length, fullname, "=>",key)
                found = true
                break
            }
        }
        if (!found) {
            groups[name+"##"+date] = [data[i]]
        }
    }


    //write grouped elsets to file
    fs.writeFileSync(filename, JSON.stringify(groups, null, 4))
    return groups
}

let grouped_elsets: { [name: string]: Elset[] } = groupElsetByLooseName(all_data,true)

//get nmae of groups and the length of their element and sort it
let grouped_elset_distribution = Object.entries(grouped_elsets).map(([key, value]) => [key, value.length]).filter((e: any) => e[1] >= 3).sort((a: any, b: any) => b[1] - a[1])

fs.writeFileSync("grouped_elsets_distribution.csv", grouped_elset_distribution.map(([key, value]) => key + "," + value).join("\n"))
