const fs = require("fs")

const FILEPATH = "30dernierjours_gp_decay_date.json"

const data = JSON.parse(fs.readFileSync(FILEPATH, "utf8"))

console.log("nb elem", data.length)