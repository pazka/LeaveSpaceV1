const fs = require("fs")

const file_path = "all_gp.json"

//open file 
const file = fs.readFileSync(file_path, "utf-8")

//count json elems
const json = JSON.parse(file)

//print all keys except data
Object.keys(json).forEach(key => {
    if (key != "data") {
        console.log(key, json[key])

    }
})

const count = json.data.length

console.log(count)
console.log(json.data[0])

function isSameSatelite(a, b) {
    return a.satelite == b.satelite
}

// write prettyfied fiel in new file
fs.writeFileSync("all_gp_pretty.json", JSON.stringify(json, null, 2))