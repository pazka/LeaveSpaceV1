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
//Count huw much of each OBJECT_TYPE there is
const object_types = all_data.map((el) => el.OBJECT_TYPE);
const object_types_set = [...new Set(object_types)];
//add the object_type but decayed to the list
const object_types_and_decayed = [];
all_data.forEach((el) => {
    if (is_decayed(el)) {
        object_types_and_decayed.push(el.OBJECT_TYPE + "_decayed");
    }
    else {
        object_types_and_decayed.push(el.OBJECT_TYPE);
    }
});
const object_types_count = {};
object_types_and_decayed.forEach((el) => {
    if (object_types_count[el]) {
        object_types_count[el] += 1;
    }
    else {
        object_types_count[el] = 1;
    }
});
function decay_date_to_date(decay_date) {
    //fromat is YYYY-MM-DD
    const parts = decay_date.split('-');
    const year = parseInt(parts[0], 10);
    const month = parseInt(parts[1], 10) - 1; // Months in JavaScript are zero-based
    const day = parseInt(parts[2], 10);
    const dateObject = new Date(year, month, day);
    return dateObject;
}
function is_decayed(el) {
    //fromat is YYYY-MM-DD
    return (el.DECAY_DATE != null && el.DECAY_DATE !== "" && el.DECAY_DATE !== "0000-00-00" && decay_date_to_date(el.DECAY_DATE) < new Date());
}
console.log(object_types_count);
//print in csv with column : active and decayed and total, and each line for the object type
const lines = [];
object_types_set.forEach((key) => {
    lines.push([key, object_types_count[key], object_types_count[key + "_decayed"], (object_types_count[key] + object_types_count[key + "_decayed"])]);
});
const csv = "TYPE,ACTIVE,DECAYED,TOTAL\n" + lines.map((el) => el.join(",")).join("\n");
fs_1.default.writeFileSync("object_types_count.csv", csv);
