branch="auxiliary-tools-improvement"
for commit in $(git rev-list --reverse origin/"$branch"..HEAD); do
	git push origin "$commit":"$branch"
	sleep 1
done
