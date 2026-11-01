const COLORS = [
    "bg-indigo-100 text-indigo-700",
    "bg-emerald-100 text-emerald-700",
    "bg-amber-100 text-amber-700",
    "bg-rose-100 text-rose-700",
    "bg-sky-100 text-sky-700",
    "bg-violet-100 text-violet-700",
];

function colorFor(id: string) {
    let hash = 0;
    for (const ch of id) hash = (hash * 31 + ch.charCodeAt(0)) >>> 0;
    return COLORS[hash % COLORS.length];
}

function initialsOf(name: string) {
    const parts = name.trim().split(/\s+/);
    const letters =
        parts.length > 1 ? parts[0][0] + parts[parts.length - 1][0] : parts[0].slice(0, 2);
    return letters.toUpperCase();
}

interface Props {
    userId: string;
    displayName: string;
}

export default function Avatar({ userId, displayName }: Props) {
    return (
        <span
            title={displayName}
            aria-label={displayName}
            className={`flex h-7 w-7 shrink-0 items-center justify-center rounded-full text-[10px] font-semibold ${colorFor(userId)}`}
        >
            {initialsOf(displayName)}
        </span>
    );
}
