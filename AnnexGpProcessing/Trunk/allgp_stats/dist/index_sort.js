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
const object_types_count = {};
object_types.forEach((el) => {
    if (object_types_count[el]) {
        object_types_count[el] += 1;
    }
    else {
        object_types_count[el] = 1;
    }
});
console.log(object_types_count);
