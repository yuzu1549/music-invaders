import OpenAI from "openai";

const {
  GITHUB_TOKEN,
  OPENAI_API_KEY,
  DISCORD_WEBHOOK_URL,
  REPOSITORY,
  PR_NUMBER,
  PR_TITLE,
  PR_BODY,
  PR_URL,
  PR_AUTHOR,
} = process.env;

if (!GITHUB_TOKEN) throw new Error("GITHUB_TOKEN is missing.");
if (!OPENAI_API_KEY) throw new Error("OPENAI_API_KEY is missing.");
if (!DISCORD_WEBHOOK_URL) throw new Error("DISCORD_WEBHOOK_URL is missing.");

const [owner, repo] = REPOSITORY.split("/");

async function githubApi(path) {
  const res = await fetch(`https://api.github.com${path}`, {
    headers: {
      Authorization: `Bearer ${GITHUB_TOKEN}`,
      Accept: "application/vnd.github+json",
      "X-GitHub-Api-Version": "2022-11-28",
    },
  });

  if (!res.ok) {
    throw new Error(`GitHub API error: ${res.status} ${await res.text()}`);
  }

  return res.json();
}

function truncate(text, maxLength) {
  if (!text) return "";
  if (text.length <= maxLength) return text;
  return text.slice(0, maxLength) + "\n...省略";
}

const commits = await githubApi(
  `/repos/${owner}/${repo}/pulls/${PR_NUMBER}/commits`
);

const files = await githubApi(
  `/repos/${owner}/${repo}/pulls/${PR_NUMBER}/files`
);

const commitMessages = commits
  .map((commit) => `- ${commit.commit.message.split("\n")[0]}`)
  .join("\n");

const changedFiles = files
  .map((file) => {
    return `- ${file.filename} (${file.status}, +${file.additions}/-${file.deletions})`;
  })
  .join("\n");

const diffSummary = files
  .slice(0, 20)
  .map((file) => {
    return `
## ${file.filename}
status: ${file.status}
additions: ${file.additions}
deletions: ${file.deletions}
patch:
${file.patch ?? "patchなし。バイナリファイル、または差分が大きい可能性があります。"}
`;
  })
  .join("\n");

const prompt = `
あなたはGitHub Pull Requestの内容をチーム向けに要約するアシスタントです。
以下のPR情報をもとに、Discordに投稿する日本語の説明文を作成してください。

条件:
- 差分から分かることだけを書く
- 推測しすぎない
- チームメンバーがレビューしやすいようにまとめる
- 長すぎないようにする

出力形式:
【概要】
【主な変更点】
【影響しそうな箇所】
【レビュー時に見てほしい点】
【コミット文から読み取れる意図】

PR情報:
リポジトリ: ${REPOSITORY}
PR番号: #${PR_NUMBER}
PRタイトル: ${PR_TITLE}
PR作成者: ${PR_AUTHOR}
PR本文:
${PR_BODY || "本文なし"}

コミットメッセージ:
${commitMessages || "コミットメッセージなし"}

変更ファイル:
${changedFiles || "変更ファイルなし"}

差分:
${truncate(diffSummary, 12000)}
`;

const client = new OpenAI({
  apiKey: OPENAI_API_KEY,
});

const response = await client.responses.create({
  model: "gpt-4.1-mini",
  input: prompt,
});

const summary = response.output_text;

const message = {
  username: "PR Summary Bot",
  content: truncate(
    `📌 **Pull Request Summary**

**${PR_TITLE}**
${PR_URL}

作成者: ${PR_AUTHOR}

${summary}`,
    1900
  ),
};

const discordRes = await fetch(DISCORD_WEBHOOK_URL, {
  method: "POST",
  headers: {
    "Content-Type": "application/json",
  },
  body: JSON.stringify(message),
});

if (!discordRes.ok) {
  throw new Error(
    `Discord Webhook error: ${discordRes.status} ${await discordRes.text()}`
  );
}

console.log("PR summary sent to Discord.");