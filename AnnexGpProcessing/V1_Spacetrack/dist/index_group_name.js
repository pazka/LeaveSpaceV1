"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const fs_1 = __importDefault(require("fs"));
/*
Group all elset by name and same launch ate to create constellation

Possibility to modify the code and obtain groups by name alone which crate larges ocnstelations
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
function areNamesLooselyTheSame(name1, name2, max_errors, range) {
    let errors = 0;
    let names1 = name1.split(/[\s-_#()/]/g).map(n => n.trim().replace(/[\d]/g, '')).filter(x => x);
    let names2 = name2.split(/[\s-_#()/]/g).map(n => n.trim().replace(/[\d]/g, '')).filter(x => x);
    if (names1.find(n => names2.includes(n)) || names2.find(n => names1.includes(n))) {
        return true;
    }
    return false;
    for (let i = 0; i < name1.length; i++) {
        let start_index = i - range < 0 ? 0 : i - range;
        let end_index = i + range > name2.length ? name2.length : i + range;
        let sub_name = name2.substring(start_index, end_index);
        if (sub_name.includes(name1[i])) {
            continue;
        }
        else {
            errors++;
        }
        if (errors > max_errors) {
            return false;
        }
    }
    return errors <= max_errors;
}
function groupElsetByLooseName(data, dontUseCache = false) {
    let groups = {};
    const filename = "grouped_elsets.json";
    //if result file exist, just read it
    if (!dontUseCache && fs_1.default.existsSync(filename)) {
        return JSON.parse(fs_1.default.readFileSync(filename, "utf-8"));
    }
    let alreadyprocessedNames = [];
    for (let i = 0; i < data.length; i++) {
        console.log(i, "/", data.length);
        let fullname = data[i].OBJECT_NAME;
        let name = fullname.replace(/[\d\s]/g, "");
        let date = data[i].LAUNCH_DATE;
        let found = false;
        for (let key in groups) {
            let key_name = key.split('##')[0];
            let key_date = key.split('##')[1];
            if (key_date == date && areNamesLooselyTheSame(name, key_name, 4, 2)) {
                groups[key].push(data[i]);
                // console.log("found",groups[key].length, fullname, "=>",key)
                found = true;
                break;
            }
        }
        if (!found) {
            groups[name + "##" + date] = [data[i]];
        }
    }
    //write grouped elsets to file
    fs_1.default.writeFileSync(filename, JSON.stringify(groups, null, 4));
    return groups;
}
let grouped_elsets = groupElsetByLooseName(all_data, true);
//get nmae of groups and the length of their element and sort it
let grouped_elset_distribution = Object.entries(grouped_elsets).map(([key, value]) => [key, value.length]).filter((e) => e[1] >= 3).sort((a, b) => b[1] - a[1]);
fs_1.default.writeFileSync("grouped_elsets_distribution.csv", grouped_elset_distribution.map(([key, value]) => key + "," + value).join("\n"));
