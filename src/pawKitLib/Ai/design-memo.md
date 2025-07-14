# Design memo for the AI directory

## Welcome to the Museum of My Coding Regrets

If you’re reading this, congratulations: you’ve found the place where good intentions go to die. This directory is a living monument to the YAGNI principle—You Aren’t Gonna Need It—because, trust me, I’ve needed almost nothing I ever over-engineered. If you spot something clever, it’s probably a fossil from a time when I thought abstraction was cool and not a gateway to existential dread.

## YAGNI: The Only Principle That Survived

- **No Premature Abstraction:** If you see an abstract class, it’s either a mistake or a cry for help. I only generalize when the universe forces me to, and even then, I do it kicking and screaming.
- **No Overengineering:** Every class is as dumb as it needs to be. If you find a generic factory, please delete it before it breeds.
- **Experimental Data:** For all the weird, temporary, or “maybe one day” fields, there’s `ExtraProperties` in `DynamicDto`. It’s a dictionary, it’s always there, and it’s the junk drawer of this codebase. Don’t put secrets in it unless you want the code to laugh at you in production.

## Why I Don’t Abstract (Anymore)

Once upon a time, I tried to find what was common between services. Turns out, the only thing they shared was my confusion and a burning desire to be different. OpenAI, Anthropic, Google—they’re all special snowflakes. Abstraction led to wasted time, lost weekends, and a deep sense of regret. Now, I just write what I need, when I need it. If you want abstraction, there are plenty of textbooks. This is the wild west.

## Naming: Lower Your Expectations

Names are chosen for clarity and immediate need, not for perfect consistency. If you see `OpenAiAudioTranscriber` next to `GoogleMagicThingy`, don’t panic. If a class name is missing a word you think is important, just take a deep breath and move on. Consistency is a luxury for people who don’t have deadlines.

## Implementation Strategy (Or Lack Thereof)

- Build only what’s needed, when it’s needed.
- Use `ExtraProperties` for all your experimental, optional, or “I’ll clean this up later” data.
- Avoid abstraction unless the codebase starts sending you threatening emails.
- Accept naming inconsistencies as a badge of honor. If you crave order, this directory will break your spirit.

## Summary: Embrace the Chaos

This directory is a record of practical, needs-driven development—and a graveyard for my failed attempts at being clever. The goal is to ship working features fast, without getting lost in theoretical design or unnecessary abstraction. Consistency and abstraction will only be introduced when they provide clear, tangible benefits. Until then, enjoy the chaos, and remember: YAGNI forever.
