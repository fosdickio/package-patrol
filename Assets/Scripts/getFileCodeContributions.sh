for i in $(find . -name "*.cs")
do
   contribution=$(git blame --line-porcelain $i | sed -n 's/author //p' | sort | uniq -c | sort -rn | awk '{$1=$1};1' | paste -sd "," - | sed 's/[0-9]*//g')
   echo "$i, $contribution" 
done > contrib.txt