"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const fs_1 = __importDefault(require("fs"));
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
function groupElsetByLooseName(data) {
    let groups = {};
    for (let i = 0; i < data.length; i++) {
        let name = data[i].OBJECT_NAME;
        let found = false;
        for (let key in groups) {
            if (areNamesLooselyTheSame(name, key, 2, 3)) {
                groups[name].concat([data[i]]);
                found = true;
                break;
            }
        }
        if (!found) {
            groups[name] = [data[i]];
        }
    }
    return groups;
}
const grouped_elsets = groupElsetByLooseName(all_data);
console.log("grouped elsets > 2", Object.values(grouped_elsets).filter(g => g.length > 2).length);
//write grouped elsets to file
fs_1.default.writeFileSync("grouped_elsets.json", JSON.stringify(grouped_elsets));
